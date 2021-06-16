# coding=utf-8
import unittest
from unittest.mock import patch

import os

from commands import GoogleImageSearchCommand
from config import Config
from slack_channel import GoogleImageEventHandler

test_prefix = "_"
valid_event = {"text": test_prefix + "gi a valid google command"}
invalid_event = {"text": "this is missing a valid command prefix"}
help_event = {"text": test_prefix + "gi -?"}
google_response = "http://www.test.com/someimage.jpg"


@patch.object(Config, "get_config_value")
@patch.object(Config, "get_prefix", return_value=test_prefix)
class TestGoogleEventHandler(unittest.TestCase):

    def test_handles_correct_event(self, prefix_method, config_method):
        handler = GoogleImageEventHandler()
        can_handle = handler.can_handle(valid_event)
        self.assertTrue(can_handle)

    def test_does_not_handle_different_event(self, prefix_method, config_method):
        handler = GoogleImageEventHandler()
        can_handle = handler.can_handle(invalid_event)
        self.assertFalse(can_handle)

    @patch.object(GoogleImageEventHandler, "_send_message_response")
    @patch.object(GoogleImageSearchCommand, "execute", return_value = google_response)
    def test_command_execute_is_called(self, execute_method, response_method, config_method, prefix_method):
        handler = GoogleImageEventHandler()
        handler.handle(valid_event)
        execute_method.assert_called_once_with(valid_event["text"][4:])

    @patch.object(GoogleImageSearchCommand, "execute", return_value=google_response)
    @patch.object(GoogleImageEventHandler, "_send_message_response")
    def test_command_result_is_correctly_built(self, response_method, execute_method, config_method, prefix_method):
        handler = GoogleImageEventHandler()
        handler.handle(valid_event)
        response_method.assert_called_once_with(google_response, valid_event)

    @patch.object(GoogleImageSearchCommand, "execute", return_value=None)
    @patch.object(GoogleImageEventHandler, "_send_message_response")
    def test_no_result_message_correctly_sent(self, response_method, execute_method, config_method, prefix_method):
        handler = GoogleImageEventHandler()
        handler.handle(valid_event)
        empty_results_message = "No results found for \""+ valid_event["text"][4:]+"\""
        response_method.assert_called_once_with(empty_results_message, valid_event)

    @patch.object(GoogleImageSearchCommand, "get_purpose", return_value="Googles")
    @patch.object(GoogleImageEventHandler, "_send_message_response")
    def test_get_usage(self, response_method, purpose_method, config_method, prefix_method):
        handler = GoogleImageEventHandler()
        handler.handle(help_event)
        expected_help = "Google Image" + os.linesep + \
                        "Googles"+ os.linesep +\
                        "Usage: `" + test_prefix + "gi <query>" + "`"
        purpose_method.assert_called_once_with()
        response_method.assert_called_once_with(expected_help, help_event)

if __name__ == '__main__':
    unittest.main()
