from commands import GetCurrentNetKarmaCommand
from slack_channel.abstract_event_handler import AbstractEventHandler


class GetCurrentNetKarmaEventHandler(AbstractEventHandler):
    @property
    def command(self):
        return GetCurrentNetKarmaCommand()

    def _get_command_symbol(self):
        return "karma"

    def get_usage(self):
        return self.command_trigger + "[recipient <if blank, will default to you>]"

    def can_handle(self, slack_event):
        text = slack_event["text"]
        return text.startswith(self.command_trigger[:1])

    def _invoke_handler_logic(self, slack_event):
        recipient = slack_event["text"][len(self.command_trigger):]
        if recipient.startswith("@"):
            recipient = recipient[1:]

        if (recipient == ""):
            recipient = slack_event["user"]

        c = self.command
        result = c.execute(recipient)

        response_message = recipient + ": " + str(result)
        self._send_response(response_message, slack_event)
