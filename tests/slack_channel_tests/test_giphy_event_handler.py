# coding=utf-8
import unittest
from unittest.mock import patch

import os

from commands import GiphyTranslateCommand
from config import Config
from slack_channel import GiphyEventHandler

test_prefix = "_"
valid_event = {"text": test_prefix + "gif a valid Giphy command"}
invalid_event = {"text": "this is missing a valid command prefix"}
help_event = {"text": test_prefix + "gif -?"}
giphy_response = "https://www.giphy.com/a.gif"


@patch.object(Config, "get_config_value")
@patch.object(Config, "get_prefix", return_value=test_prefix)
class TestGiphyEventHandler(unittest.TestCase):

    def test_handles_correct_event(self, prefix_method, config_method):
        handler = GiphyEventHandler()
        can_handle = handler.can_handle(valid_event)
        self.assertTrue(can_handle)

    def test_does_not_handle_different_event(self, prefix_method, config_method):
        handler = GiphyEventHandler()
        can_handle = handler.can_handle(invalid_event)
        self.assertFalse(can_handle)

    @patch.object(GiphyEventHandler, "_send_message_response")
    @patch.object(GiphyTranslateCommand, "execute", return_value = giphy_response)
    def test_command_execute_is_called(self, execute_method, response_method, config_method, prefix_method):
        handler = GiphyEventHandler()
        handler.handle(valid_event)
        execute_method.assert_called_once_with(valid_event["text"][5:])

    @patch.object(GiphyTranslateCommand, "execute", return_value=giphy_response)
    @patch.object(GiphyEventHandler, "_send_message_response")
    def test_command_result_is_correctly_built(self, response_method, execute_method, config_method, prefix_method):
        handler = GiphyEventHandler()
        handler.handle(valid_event)
        response_method.assert_called_once_with(giphy_response, valid_event)

    @patch.object(GiphyTranslateCommand, "execute", return_value=None)
    @patch.object(GiphyEventHandler, "_send_message_response")
    def test_no_result_message_correctly_sent(self, response_method, execute_method, config_method, prefix_method):
        handler = GiphyEventHandler()
        handler.handle(valid_event)
        empty_results_message = "No gifs found for \"" + valid_event["text"][5:] + "\""
        response_method.assert_called_once_with(empty_results_message, valid_event)

    @patch.object(GiphyTranslateCommand, "get_purpose", return_value="Giphys")
    @patch.object(GiphyEventHandler, "_send_message_response")
    def test_get_usage(self, response_method, purpose_method, config_method, prefix_method):
        handler = GiphyEventHandler()
        handler.handle(help_event)
        expected_help = "Giphy" + os.linesep + \
                        "Giphys" + os.linesep + \
                        "Usage: `" + test_prefix + "gif <query>" + "`"
        purpose_method.assert_called_once_with()
        response_method.assert_called_once_with(expected_help, help_event)

if __name__ == '__main__':
    unittest.main()