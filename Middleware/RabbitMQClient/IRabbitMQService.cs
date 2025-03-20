using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Middleware.RabbitMQClient
{
    public interface IRabbitMQService
    {
        void SendMessage(string message);
        void ReceiveMessage();
    }
}
