using System;

namespace gameSDK
{
    public class DefActorAction: ActorAction
    {
        internal Action<DefActorAction> stopAction;
        public object args;
        public DefActorAction(int stateID):base(stateID)
        {
        }


        protected override void doStart()
        {
            _isFinished = false;
            base.doStart();
        }

        protected override void doStop()
        {
            if (stopAction != null)
            {
                stopAction(this);
            }
            base.doStop();
        }
    }
}