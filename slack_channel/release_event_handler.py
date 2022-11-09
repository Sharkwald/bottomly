from commands import ReleaseCommand
from slack_channel.abstract_event_handler import AbstractEventHandler


command_symbol = "release"


class ReleaseEventHandler(AbstractEventHandler):
    @property
    def command(self) -> ReleaseCommand:
        return ReleaseCommand()

    @property
    def name(self):
        return "Release"

    def get_usage(self):
        return self.command_trigger[:-1]

    def can_handle(self, slack_event):
        text = slack_event["text"]
        return text.startswith(self.command_trigger[:-1])

    def _invoke_handler_logic(self, slack_event):
        release_desc = self.command.execute()
        if release_desc is None:
            self._send_message_response('Unable to retrieve latest release info.', slack_event)
        else:
            self._send_message_response(release_desc, slack_event)

    def _get_command_symbol(self):
        return command_symbol