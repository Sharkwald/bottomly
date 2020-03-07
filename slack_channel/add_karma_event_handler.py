import logging
from abc import abstractmethod

from commands import AddKarmaCommand
from config import Config
from model.karma import KarmaType
from model.member import Member
from slack_channel.abstract_event_handler import AbstractEventHandler
from slack_channel.slack_parser import SlackParser


class AbstractKarmaEventHandler(AbstractEventHandler):

    FOR_STRING = " for "

    @property
    @abstractmethod
    def name(self) -> str:
        pass

    @abstractmethod
    def _get_command_symbol(self) -> str:
        pass

    @property
    def command(self) -> AddKarmaCommand:
        return AddKarmaCommand()

    @property
    @abstractmethod
    def karma_type(self) -> KarmaType:
        pass

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
            self.command.execute(awarded_to=args["recipient"],
                                 awarded_by=slack_event["user"],
                                 reason=args["reason"],
                                 karma_type=args["karma_type"])
            self._send_reaction_response(slack_event)
        except Exception as ex:
            logging.exception(ex)

    def _parse_command_text(self, command_text):
        command_text = SlackParser.replace_slack_id_tokens_with_usernames(command_text)
        command_text = command_text[len(self._get_command_symbol()):].lstrip()

        command_split = command_text.split(self.FOR_STRING) # Attempt to tokenise on "for"
        if len(command_split) > 1: # If there's a "for" then life is easy
            recipient = command_split[0] # First token is the username
        else:
            recipient = self._parse_recipient(command_text) # Get the recipient out

        # Get the reason by removing the first occurrence of the recipient from the command_text
        reason = command_text.replace(recipient, "", 1)
        if reason.startswith(self.FOR_STRING):
            # If Command starts with for then remove it
            reason = reason.replace(self.FOR_STRING, "", 1).lstrip()
        else:
            reason = reason.lstrip()

        return {"recipient": recipient, "reason": reason, "karma_type": self.karma_type}

    def _parse_recipient(self, command_text):
        possible_username = command_text.split(" ")[0] # split on spaces, first word is a possible user
        decided_username = command_text # assume we're going to be using the full command_text as the username

        username_is_known = self._username_is_known(possible_username)
        if username_is_known: # if user is known in the DB then it is a usable name
            decided_username = possible_username

        return decided_username

    @staticmethod
    def _username_is_known(username):
        m = Member.get_member_by_username(username)
        if m is not None:
            return True
        else:
            return False

    @staticmethod
    def _parse_reason(command_text, recipient):
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

    @property
    def name(self):
        return "Pozzy-poz"

    def _get_command_symbol(self):
        return "++"

    @property
    def karma_type(self):
        return KarmaType.POZZYPOZ

    def __init__(self, debug=False):
        super(IncrementKarmaEventHandler, self).__init__(debug)


class DecrementKarmaEventHandler(AbstractKarmaEventHandler):

    @property
    def name(self):
        return "Neggy-neg"

    def _get_command_symbol(self):
        return "--"

    @property
    def karma_type(self):
        return KarmaType.NEGGYNEG

    def __init__(self, debug=False):
        self.debug = debug
        super(DecrementKarmaEventHandler, self).__init__(debug)
