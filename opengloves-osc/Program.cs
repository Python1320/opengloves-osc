
using GloveInputLib;
using Rug.Osc;

public class Program
{

    static OscAddressManager m_Listener = default!;
    static OscReceiver receiver = default!;
    static Thread thread = default!;
    static InputData input;
    static GloveInputLink inputLink = default!;
    static void Main(string[] args)
    {
        
        inputLink = new GloveInputLink(GloveInputLink.Handness.Left);

        float[] flextion = new float[20] { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f };

        float[] splay = new float[5] { 0.5f, 0.5f, 0.5f, 0.5f, 0.5f };


        input = new InputData(
            flextion,
            splay,
            0,
            0,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            0
        );



        int port = 9007;
        receiver = new OscReceiver(port);

        receiver.Connect();

        m_Listener = new OscAddressManager();

        m_Listener.Attach("/input/Horizontal", f_joyX);
        m_Listener.Attach("/input/Vertical", f_joyY);
        m_Listener.Attach("/button/trigger", f_trgButton);
        m_Listener.Attach("/button/a", f_aButton);
        m_Listener.Attach("/button/b", msg => { input.bButton = Convert.ToBoolean(msg[0]); send(); });
        m_Listener.Attach("/button/joy", msg => { input.joyButton = Convert.ToBoolean(msg[0]); send(); });
        m_Listener.Attach("/button/grab", f_grab);
        m_Listener.Attach("/button/menu", f_menu);
        try
        {
            while (receiver.State != OscSocketState.Closed)
            {
                if (receiver.State == OscSocketState.Connected)
                {
                    OscPacket packet = receiver.Receive();

                    switch (m_Listener.ShouldInvoke(packet))
                    {
                        case OscPacketInvokeAction.Invoke:
                           
                            if (m_Listener.Invoke(packet))
                            {
                                Console.WriteLine("Unhandled packet: {0}",packet);
                            }
                            break;
                        case OscPacketInvokeAction.DontInvoke:
                            Console.WriteLine("Cannot invoke");
                            Console.WriteLine(packet.ToString());
                            break;
                        case OscPacketInvokeAction.HasError:
                            Console.WriteLine("Error reading osc packet, " + packet.Error);
                            Console.WriteLine(packet.ErrorMessage);
                            break;
                        case OscPacketInvokeAction.Pospone:
                            Console.WriteLine("Postponed bundle");
                            Console.WriteLine(packet.ToString());
                            break;
                        default:
                            break;
                    }
                    
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception in listen loop");
            Console.WriteLine(ex.Message);
        }

        receiver.Close();

    }

    static void f_joyX(OscMessage message)
    {
        float x = (float)message[0];
        input.joyX = x; send();
    }
    static void f_joyY(OscMessage message)
    {
        float val = (float)message[0];
        input.joyY = val; send();
    }
    static void f_grab(OscMessage message)
    {
        bool val = Convert.ToBoolean(message[0]);
        input.grab = val; send();
    }
    static void f_aButton(OscMessage message)
    {
        Console.WriteLine(message);
        bool val = Convert.ToBoolean(message[0]);
        input.aButton = val;
        Console.WriteLine("abutton == {0}",input.aButton.ToString() ?? "null");
        send();
    }
    static void f_bButton(OscMessage message)
    {
        bool val = Convert.ToBoolean(message[0]);
        input.bButton = val; send();
    }

    static void f_menu(OscMessage message)
    {
        bool val = Convert.ToBoolean(message[0]);
        input.menu = val; send();
    }

    static void f_trgButton(OscMessage message)
    {
        bool val = Convert.ToBoolean(message[0]);
        input.trgButton = val;
        send();
    }

    private static void send()
    {
        inputLink.Write(input);
    }
}