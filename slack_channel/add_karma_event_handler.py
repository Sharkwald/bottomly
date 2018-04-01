import logging

from commands import AddKarmaCommand
from config import Config
from model.karma import KarmaType
from model.member import Member
from slack_channel.abstract_event_handler import AbstractEventHandler
from slack_channel.slack_parser import SlackParser


class AbstractKarmaEventHandler(AbstractEventHandler):

    @property
    def command(self):
        return AddKarmaCommand()

    def get_usage(self):
        return self._get_command_symbol() + " recipient [[for <if recipient is not a known user>] reason]"

    @property
    def _help_message(self):
        return self._get_command_symbol() + " -?"

    def can_handle(self, slack_event):
        text = slack_event["text"]
        return text.startswith(self._get_command_symbol())

    def _invoke_handler_logic(self, slack_event):
        try:
            command_text = slack_event['text']
            args = self._parse_command_text(command_text)
            c = self.command
            c.execute(awarded_to=args["recipient"],
                      awarded_by=slack_event["user"],
                      reason=args["reason"],
                      karma_type=args["karma_type"])
            self._send_reaction_response(slack_event)
        except Exception as ex:
            logging.exception(ex)

    def _parse_command_text(self, command_text):
        command_text = SlackParser.replace_slack_id_tokens_with_usernames(command_text)
        karma_type_arg = command_text[:2]
        karma_type = KarmaType.POZZYPOZ if karma_type_arg == "++" else KarmaType.NEGGYNEG
        command_text = command_text[3:]

        if command_text.find(" for ") != -1:
            command_split = command_text.split(" for ")
            recipient = self._parse_recipient(command_split[0].split(" "))
        else:
            command_split = command_text.split(" ")
            recipient = self._parse_recipient(command_split)

        reason = self._parse_reason(command_text, recipient)

        return {"recipient": recipient, "reason": reason, "karma_type": karma_type}

    def _parse_recipient(self, command_split):
        possible_username = command_split[0]
        decided_username = " ".join(command_split)

        username_is_known = self._username_is_known(possible_username)
        if username_is_known:
            decided_username = possible_username

        return decided_username

    def _username_is_known(self, username):
        m = Member.get_member_by_username(username)
        if m is not None:
            return True
        else:
            return False

    def _parse_reason(self, command_text, recipient):
        recipient_length = len(recipient) + 1  # +1 to account for space
        if command_text.find(" for ") != -1:
            recipient_length += 4
        reason = command_text[recipient_length:]
        return reason

    def __init__(self, debug=False):
        self.config = Config()
        self.config.connect_to_db()
        super(AbstractKarmaEventHandler, self).__init__(debug)


class IncrementKarmaEventHandler(AbstractKarmaEventHandler):

    def _get_command_symbol(self):
        return "++"

    def __init__(self, debug=False):
        super(IncrementKarmaEventHandler, self).__init__(debug)


class DecrementKarmaEventHandler(AbstractKarmaEventHandler):

    def _get_command_symbol(self):
        return "--"

    def __init__(self, debug=False):
        self.debug = debug
        super(DecrementKarmaEventHandler, self).__init__(debug)
