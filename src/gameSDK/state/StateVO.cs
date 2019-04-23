using System.Collections.Generic;

namespace gameSDK
{
    public class StateVO
    {
        protected int stateType;
        public string name;
        private List<int> _terminates = new List<int>();
        private List<int> _limits = new List<int>();

        public List<int> terminates
        {
            get { return _terminates; }
        }
        public List<int> limits
        {
            get { return _limits; }
        }

        public StateVO(int type)
        {
            this.stateType = type;
        }

        public StateVO addTerminates(params int[] args)
        {
            foreach (int i in args)
            {
                if (_terminates.IndexOf(i) == -1)
                {
                    _terminates.Add(i);
                }
            }
            return this;
        }

        public StateVO addAdvTerminates(List<int> list,params int[] singleOtherArgs)
        {
            foreach (int i in list)
            {
                if (_terminates.IndexOf(i) == -1)
                {
                    _terminates.Add(i);
                }
            }
            foreach (int i in singleOtherArgs)
            {
                if (_terminates.IndexOf(i) == -1)
                {
                    _terminates.Add(i);
                }
            }
            return this;
        }

        public StateVO addLimits(params int[] args)
        {
            foreach (int i in args)
            {
                if (_limits.IndexOf(i) == -1)
                {
                    _limits.Add(i);
                }
            }
            return this;
        }
        public StateVO addAdvLimits(List<int> list,params int[] singleOtherArgs)
        {
            foreach (int i in list)
            {
                if (_limits.IndexOf(i) == -1)
                {
                    _limits.Add(i);
                }
            }
            foreach (int i in singleOtherArgs)
            {
                if (_limits.IndexOf(i) == -1)
                {
                    _limits.Add(i);
                }
            }
            return this;
        }


        public void clear()
        {
            _terminates.Clear();
            _limits.Clear();
        }
    }
}