using System;
using System.Runtime.InteropServices;

namespace ASCOM.NexStar
{
    [ComVisibleAttribute(false)] /* fixes generic type warning */
    internal class EventArgs<T> : EventArgs
    {
        public EventArgs(T value)
        {
            m_value = value;
        }

        private T m_value;

        public T Value
        {
            get { return m_value; }
        }
    }

    [ComVisibleAttribute(false)] /* fixes generic type warning */
    internal class EventArgs<Ta, Tb> : EventArgs
    {
        public EventArgs(Ta a, Tb b)
        {
            a_value = a;
            b_value = b;
        }

        private Ta a_value;
        private Tb b_value;

        public Ta ValueA
        {
            get { return a_value; }
        }

        public Tb ValueB
        {
            get { return b_value; }
        }

    }

    [ComVisibleAttribute(false)] /* fixes generic type warning */
    internal class EventArgs<Ta, Tb, Tc> : EventArgs
    {
        public EventArgs(Ta a, Tb b, Tc c)
        {
            a_value = a;
            b_value = b;
            c_value = c;
        }

        private Ta a_value;
        private Tb b_value;
        private Tc c_value;

        public Ta ValueA
        {
            get { return a_value; }
        }

        public Tb ValueB
        {
            get { return b_value; }
        }

        public Tc ValueC
        {
            get { return c_value; }
        }

    }
}
