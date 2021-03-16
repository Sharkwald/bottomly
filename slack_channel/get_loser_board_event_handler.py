import os

from commands import GetLoserBoardCommand
from slack_channel.abstract_event_handler import AbstractEventHandler


command_symbol = "loserboard"


def _build_response(result: list) -> str:
    # result example: [{"username": "<username>", "net_karma": <karma value>}]
    response = ""
    for r in result:
        response += "{username}: {net_karma}".format(username=r["username"], net_karma=r["net_karma"])
        response += os.linesep
    return response.strip()


class GetLoserBoardEventHandler(AbstractEventHandler):
    @property
    def command(self) -> GetLoserBoardCommand:
        return GetLoserBoardCommand()

    @property
    def name(self):
        return "Get Loserboard"

    def _get_command_symbol(self):
        return command_symbol

    def get_usage(self):
        return self.command_trigger + "[size of loserboard. Default is 3]"

    def can_handle(self, slack_event):
        text = slack_event["text"]
        return text.startswith(self.command_trigger[:-1])  # Trim the space off in case of no size arg

    def _invoke_handler_logic(self, slack_event):
        message = slack_event["text"]
        size = 3
        size_arg = message[len(self.command_trigger):]
        if size_arg.isdigit() and int(size_arg) > 0:
            size = int(size_arg)

        result = self.command.execute(size)

        response_message = _build_response(result)
        self._send_message_response(response_message, slack_event)
