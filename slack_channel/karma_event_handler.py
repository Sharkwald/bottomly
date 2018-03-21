import logging

from pymodm import errors
from slacker import Slacker

from commands import AddKarmaCommand
from config import Config, ConfigKeys
from model.karma import KarmaType
from model.member import Member
from slack_channel.abstract_event_handler import AbstractEventHandler


class AbstractKarmaEventHandler(AbstractEventHandler):

    @property
    def command(self):
        return AddKarmaCommand()

    def get_usage(self):
        return self.command_trigger + "recipient [[for <if recipient is not a known user>] reason]"

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
        except Exception as ex:
            logging.exception(ex)

    def _parse_command_text(self, command_text):
        karma_type_arg = command_text[:2]
        karma_type = KarmaType.POZZYPOZ if karma_type_arg == "++" else KarmaType.NEGGYNEG
        command_text = command_text[2:]

        if command_text.find(" for ") != -1:
            command_split = command_text.split(" for ")
            recipient = self._parse_recipient(command_split[0].split(" "))
            reason = command_text[len(recipient + " for "):]

        else:
            command_split = command_text.split(" ")
            recipient = self._parse_recipient(command_split)
            reason = command_text[len(recipient + " "):]

        return {"recipient": recipient, "reason": reason, "karma_type": karma_type}

    def _parse_recipient(self, command_split):
        possible_username = command_split[0]
        username_is_known = self._username_is_known(possible_username)
        if username_is_known:
            return possible_username
        return " ".join(command_split)

    def _username_is_known(self, username):
        try:
            Member.objects.raw({'_id': username})
            return True
        except errors.DoesNotExist:
            return False

    def _send_response(self, response_message, slack_event):
        """We want to indicate that we've added the karma, but not send a message, so instead doing the usual response,
        we'll add a reaction to the original message"""
        try :
            slack = Slacker(Config().get_config_value(ConfigKeys.slack_bot_token))
            slack.reactions.add(name="robot_face",
                                channel=slack_event["channel"],
                                timestamp=slack_event["ts"])
        except Exception as ex:
            logging.exception(ex)

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
