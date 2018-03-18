# coding=utf-8
import unittest
from unittest.mock import patch

from commands import GoogleSearchCommand
from config import Config
from slack_channel.google_event_handler import GoogleEventHandler

valid_event = {"text": "_g a valid google command", "channel": "testChannel"}
invalid_event = {"text": "this is missing a '_g' prefix"}

class TestGoogleEventHandler(unittest.TestCase):

    def test_handles_correct_event(self):
        handler = GoogleEventHandler()
        can_handle = handler.can_handle(valid_event)
        self.assertTrue(can_handle)

    def test_does_not_handle_different_event(self):
        handler = GoogleEventHandler()
        can_handle = handler.can_handle(invalid_event)
        self.assertFalse(can_handle)

    @patch.object(Config, "get_config_value")
    @patch.object(GoogleSearchCommand, "execute", return_value = {"title": "response title", "link": "response_link"})
    def test_command_execute_is_called(self, execute_method, config_method):
        handler = GoogleEventHandler()
        handler.handle(valid_event)
        execute_method.assert_called_with(valid_event["text"][3:])

    @patch.object(Config, "get_config_value")
    @patch.object(GoogleSearchCommand, "execute", return_value={"title": "response title", "link": "response_link"})
    @patch.object(GoogleEventHandler, "_send_google_response")
    def test_command_result_is_correctly_built(self, response_method, execute_method, config_method):
        handler = GoogleEventHandler()
        handler.handle(valid_event)
        response_method.assert_called_with("response title response_link", valid_event)

    if __name__ == '__main__':
        unittest.main()