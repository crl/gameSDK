using foundation;
using System;
using System.Reflection;
using UnityEngine;

namespace gameSDK
{
    public class MVCInject : IInject
    {
        private IFacade facade;
        public MVCInject(IFacade facade)
        {
            this.facade = facade;
        }
        public object inject(object injectable)
        {
            Type contract = injectable.GetType();

            FieldInfo[] fields = contract.GetFields(BindingFlags.Instance | BindingFlags.Public |
                                                    BindingFlags.NonPublic);

            Type m = typeof(MVCAttribute);
            IMediator mediator = injectable as IMediator;
            PanelDelegate panelDelegate = null;
            if (mediator == null)
            {
                panelDelegate = injectable as PanelDelegate;
            }
            int len = fields.Length;
            for (int i = 0; i < len; i++)
            {
                FieldInfo info = fields[i];
                //获取成员变量的属性
                object[] attrs = info.GetCustomAttributes(m, true);
                int alen = attrs.Length;
                for (int j = 0; j < alen; j++)
                {
                    object attr = attrs[j];
                    //只遍历带MVC 属性的成员变量
                    if (attr is MVCAttribute == false) continue;
                    //通过autoMVC方法对成员变量赋值
                    object valueObj = autoMVC(info.FieldType);
                    if (valueObj == null) continue;
                    //如果成员变量不为空 就把它赋值给注射对象（mediator
                    info.SetValue(injectable, valueObj);
                    if (mediator == null)
                    {
                        if (panelDelegate != null)
                        {
                            if (info.Name == "model")
                            {
                                panelDelegate.setModel(valueObj as IProxy);
                            }
                        }
                        continue;
                    }
                    //如果变量名是view 就把成员变量作为panel赋值给mediator的view
                    if (info.Name == "view")
                    {
                        mediator.setView(valueObj as IPanel);
                    }
                    //如果变量名是model 就把成员变量作为proxy赋值给mediator的model
                    else if (info.Name == "model")
                    {
                        mediator.setModel(valueObj as IProxy);
                    }
                }
            }

            MethodInfo[] methods = contract.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            CMDAttribute cmdAttr;
            int code;
            Type cmdAttributeType = typeof(CMDAttribute);
            len = methods.Length;
            for (int i = 0; i < len; i++)
            {
                MethodInfo info = methods[i];
                object[] attrs = info.GetCustomAttributes(cmdAttributeType, false);
                int alen = attrs.Length;
                for (int j = 0; j < alen; j++)
                {
                    object attr = attrs[j];
                    cmdAttr = attr as CMDAttribute;
                    code = cmdAttr.code;
                    if (code < 1)
                    {
                        code = int.Parse(info.Name.Split('_')[1]);
                    }

                    SocketX.AddListener(code, (IMessageExtensible msg) =>
                    {
                        try
                        {
                            info.Invoke(injectable, new[] { msg });
                        }
                        catch (Exception e)
                        {
                            string str = "Socket Router Error:" + msg.getMessageType() + ", method:" + contract.Name +
                                         "." + info.Name;
                            str += " IMessageExtensible:" + msg.GetType().ToString();
                            str += " Error:"+e.Message;
                            Debug.LogWarning(str);
                            throw e;
                        }
                       
                    });
                }
            }

            return injectable;
        }


        protected object autoMVC(Type type)
        {
            string fullName = type.FullName;
            //获取别名
            string beanID = Singleton.getAliasName(fullName);
            object source;
            //如果别名为空（UIPanel之前没有注册过，没有别名）
            if (beanID == null)
            {
                if (type.IsSubclassOf(typeof (Component)))
                {
                    DebugX.LogError(fullName);
                    return null;
                }

                if (Singleton.isInUnique(fullName) == true)
                {
                    return Singleton.getInstance(fullName);
                }

                if (type.IsSubclassOf(typeof(Proxy)) || type.IsSubclassOf(typeof(Mediator)))
                {
                    beanID = type.Name;
                    Singleton.registerClass(type);
                    DebugX.LogWarning("mvc类请先注册 Singleton.registerClass<{0}>", beanID);
                }
                else
                {
                    //如果不是控件、不是单例（其实就是指UIPanel啦），就创建实例并返回
                    if (type.IsClass)
                    {
                        return facade.routerCreateInstance(type);
                    }
                    else
                    {
                        throw new Exception("请为非类类型指定别名:" + fullName);
                    }
                }
            }
            //如果别名不为空（之前注册过了），就判断Facede里面有没有这个Mediator或者Proxy，有的话就返回
            if (facade.hasMediator(beanID))
            {
                return facade.getMediator(beanID);
            }
            if (facade.hasProxy(beanID))
            {
                return facade.getProxy(beanID);
            }
            //看看是不是Facade中正在注射的东西，是的话就注册完了返回
            source = facade.getInjectLock(beanID);
            if (source == null)
            {
                source = Singleton.__getOneInstance(beanID);
                if (source is IInjectable)
                {
                    source = inject(source);
                }
                if (source is IMediator)
                {
                    facade.registerMediator((IMediator)source);
                }
                else if (source is IProxy)
                {
                    facade.registerProxy((IProxy)source);
                }
            }
            return source;
        }
    }
}
