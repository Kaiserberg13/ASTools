using ASTools.ModelViews;
using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASTools.Messenger
{
    public class OpenToolPageMessage : ValueChangedMessage<ToolPageViewModel>
    {
        public OpenToolPageMessage(ToolPageViewModel value) : base(value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value), "ToolPageViewModel cannot be null");
            }
        }
    }
}
