import logging
import os

from slacksocket import SlackSocket

from commands import GetCurrentKarmaReasonsCommand
from model.karma import KarmaType
from slack_channel.abstract_event_handler import AbstractEventHandler
from slack_channel.slack_parser import SlackParser


class GetCurrentKarmaReasonsEventHandler(AbstractEventHandler):
    @property
    def command(self):
        return GetCurrentKarmaReasonsCommand()

    def _get_command_symbol(self):
        return "reasons"

    def get_usage(self):
        return self.command_trigger + "[recipient <if blank, will default to you>]"

    def can_handle(self, slack_event):
        text = slack_event["text"]
        return text.startswith(self.command_trigger[:1]) # Trim the space off in case of no recipient

    def _invoke_handler_logic(self, slack_event):
        message = SlackParser.replace_slack_id_tokens_with_usernames(slack_event["text"])
        recipient = message[len(self.command_trigger):].split(' ')[0]

        if (recipient == ""):
            recipient = slack_event["user"]

        c = self.command
        result = c.execute(recipient)

        response = self._build_response(result)
        self._send_response(response, slack_event)

    def _send_response(self, response_message, slack_event):
        with SlackSocket(self.token) as s:
            dm_channel = s.get_im_channel(slack_event["user"])
            msg = s.send_msg(response_message, dm_channel["id"])
            logging.info(msg.sent)

    def _build_response(self, result):
        karma_keys = {str(KarmaType.POZZYPOZ): "++", str(KarmaType.NEGGYNEG): "--"}
        response = "Recent Karma given with no reason: " + str(result["reasonless"])
        karma_with_reasons = result["reasoned"]
        for k in karma_with_reasons:
            response += os.linesep
            response += karma_keys[k.karma_type] + " "
            response += 'for "' + k.reason + '"'

        return response
