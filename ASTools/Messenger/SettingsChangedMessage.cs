using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace ASTools.Messenger
{
    public class SettingsChangedMessage : ValueChangedMessage<bool>
    {
        public SettingsChangedMessage(bool value) : base(value) { }
    }
}
