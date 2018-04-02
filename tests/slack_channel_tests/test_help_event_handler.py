import unittest
from unittest.mock import patch, MagicMock

import os

from config import Config
from slack_channel import HelpEventHandler
from slack_channel.abstract_event_handler import AbstractEventHandler

test_prefix = "_"
valid_events = [{"text": test_prefix + "help"},{"text": test_prefix + "?"},{"text": test_prefix + "list"}]
invalid_event = {"text": "dnfjklsnjfsdl"}
help_event = {"text": "_help -?"}

@patch.object(Config, "get_config_value")
@patch.object(Config, "get_prefix", return_value=test_prefix)
class TestHelpEventHandler(unittest.TestCase):

    def test_handles_correct_event(self, prefix_method, config_method):
        handler = HelpEventHandler()
        for ve in valid_events:
            can_handle = handler.can_handle(ve)
            self.assertTrue(can_handle)

    def test_does_not_handle_different_event(self, prefix_method, config_method):
        handler = HelpEventHandler()
        can_handle = handler.can_handle(invalid_event)
        self.assertFalse(can_handle)

    @patch.object(HelpEventHandler, "_send_message_response")
    def test_get_usage(self, response_method, config_method, prefix_method):
        handler = HelpEventHandler()
        handler.handle(help_event)
        expected_help = "Help" + os.linesep + \
                        "Usage: `"+ test_prefix + "help` or `" + test_prefix + "?` or `" + test_prefix + "list`"
        response_method.assert_called_once_with(expected_help, help_event)

    def test_execution(self, config_method, prefix_method):
        message1 = "message 1"
        message2 = "message 2"
        message3 = "message 3"

        mock_handler_1 = MagicMock(AbstractEventHandler)
        mock_handler_1.build_help_message.return_value = message1
        mock_handler_2 = MagicMock(AbstractEventHandler)
        mock_handler_2.build_help_message.return_value = message2
        mock_handler_3 = MagicMock(AbstractEventHandler)
        mock_handler_3.build_help_message.return_value = message3

        mock_handlers = list([mock_handler_1, mock_handler_2, mock_handler_3])

        event = valid_events[0]

        with patch.object(HelpEventHandler, "_send_message_response") as response_method:
            handler = HelpEventHandler(command_handlers=mock_handlers)
            handler.handle(event)

            expected_message_text = message1 + os.linesep + message2 + os.linesep + message3

            response_method.assert_called_once_with(expected_message_text, event)

if __name__ == '__main__':
    unittest.main()