from commands import GetCurrentNetKarmaCommand
from slack_channel.abstract_event_handler import AbstractEventHandler
from slack_channel.slack_parser import SlackParser


command_symbol = "karma"

class GetCurrentNetKarmaEventHandler(AbstractEventHandler):
    @property
    def command(self):
        return GetCurrentNetKarmaCommand()

    @property
    def name(self):
        return "Get Current Karma"

    def _get_command_symbol(self):
        return command_symbol

    def get_usage(self):
        return self.command_trigger + "[recipient <if blank, will default to you>]"

    def can_handle(self, slack_event):
        text = slack_event["text"]
        return text.startswith(self.command_trigger[:-1]) # Trim the space off in case of no recipient

    def _invoke_handler_logic(self, slack_event):
        message = SlackParser.replace_slack_id_tokens_with_usernames(slack_event["text"])
        recipient = message[len(self.command_trigger):].split(' ')[0]

        if (recipient == ""):
            recipient = slack_event["user"]

        c = self.command
        result = c.execute(recipient)

        response_message = recipient + ": " + str(result)
        self._send_message_response(response_message, slack_event)
