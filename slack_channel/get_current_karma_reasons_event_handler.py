import os

from commands import GetCurrentKarmaReasonsCommand
from model.karma import KarmaType
from slack_channel.abstract_event_handler import AbstractEventHandler
from slack_channel.slack_parser import SlackParser

command_symbol = "reasons"


def _build_response(result, recipient) -> str:
    karma_keys = {str(KarmaType.POZZYPOZ): "++", str(KarmaType.NEGGYNEG): "--"}
    response = "Recent Karma for " + recipient + ":"
    response += os.linesep
    response += "Recently awarded with no reason: " + str(result["reasonless"]) + "."
    karma_with_reasons = result["reasoned"]
    if len(karma_with_reasons) == 0:
        response += os.linesep
        response += "None awarded with a reason given."
        return response

    for k in karma_with_reasons:
        response += os.linesep
        response += karma_keys[k.karma_type] + " "
        response += "from " + k.awarded_by_username + " "
        response += 'for "' + k.reason + '"'

    return response


class GetCurrentKarmaReasonsEventHandler(AbstractEventHandler):
    @property
    def command(self) -> GetCurrentKarmaReasonsCommand:
        return GetCurrentKarmaReasonsCommand()

    @property
    def name(self):
        return "Karma Reasons"

    def _get_command_symbol(self):
        return command_symbol

    def get_usage(self):
        return self.command_trigger + "[recipient <if blank, will default to you>]"

    def can_handle(self, slack_event):
        text = slack_event["text"]
        return text.startswith(self.command_trigger[:-1])  # Trim the space off in case of no recipient

    def _invoke_handler_logic(self, slack_event):
        message = SlackParser.replace_slack_id_tokens_with_usernames(slack_event["text"])
        recipient = message[len(self.command_trigger):].split(' ')[0]

        if recipient == "":
            recipient = slack_event["user"]

        result = self.command.execute(recipient)

        response = _build_response(result, recipient)
        self._send_dm_response(response, slack_event)
