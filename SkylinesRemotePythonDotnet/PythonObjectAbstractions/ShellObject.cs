using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkylinesRemotePython.API
{
    public abstract class ShellObject<T>
    {
        private bool inited { get; set; }

        public string initialization_error_msg { get; private set; }

        protected abstract T Retrieve();

        protected T _ {
            get {
                if (IsInitialized()) {
                    return Retrieve();
                }
                while (true) {
#if DEBUG
                    Console.WriteLine($"Waiting for initialization of {this.GetType().Name}...");
#endif
                    ClientHandler.Instance.WaitForNextMessage();
                    if (IsInitialized()) {
                        return Retrieve();
                    }
                }
            }
        }

        private bool IsInitialized()
        {
            if (inited) {
                return true;
            }
            if (initialization_error_msg != null) {
                throw new Exception(initialization_error_msg);
            }
            return false;
        }

        protected void Initialize(string error = null)
        {
            if(error != null) {
                initialization_error_msg = error;
            } else {
                inited = true;
            }
        }
    }
}
