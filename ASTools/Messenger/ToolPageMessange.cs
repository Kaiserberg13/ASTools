using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASTools.Messenger
{
    public class BackToFolderMassege : ValueChangedMessage<bool>
    {
        public BackToFolderMassege(bool value) : base(value)
        {
        }
    }
}
