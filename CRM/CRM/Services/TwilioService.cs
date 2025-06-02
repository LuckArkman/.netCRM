using Interfaces;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.TwiML;
using Twilio.Types;

namespace Services;

public class TwilioService : IMultichannelService
{
    private readonly string _accountSid;
    private readonly string _authToken;
    private readonly string _whatsAppFrom;
    private readonly string _voiceFrom;

    public TwilioService(IConfiguration configuration)
    {
        _accountSid = configuration["TwilioSettings:AccountSid"];
        _authToken = configuration["TwilioSettings:AuthToken"];
        _whatsAppFrom = configuration["TwilioSettings:WhatsAppFrom"];
        _voiceFrom = configuration["TwilioSettings:VoiceFrom"];

        TwilioClient.Init(_accountSid, _authToken);
    }

    public async Task SendWhatsAppMessageAsync(string to, string message)
    {
        await MessageResource.CreateAsync(
            to: new PhoneNumber(to),
            from: new PhoneNumber(_whatsAppFrom),
            body: message
        );
    }

    public async Task SendVoiceCallAsync(string to, string message)
    {
        var response = new VoiceResponse();
        response.Say(message, language: "pt-BR");

        await CallResource.CreateAsync(
            to: new PhoneNumber(to),
            from: new PhoneNumber(_voiceFrom),
            twiml: new Twiml(response.ToString())
        );
    }

    // Método para gerar TwiML para resposta a chamadas de voz
    public string GenerateVoiceResponseTwiML(string message)
    {
        var response = new VoiceResponse();
        response.Say(message, language: "pt-BR");
        return response.ToString();
    }
}