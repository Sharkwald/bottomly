# coding=utf-8
import unittest
from unittest.mock import patch
from commands import UrbanSearchCommand
from config import Config
from slack_channel import UrbanEventHandler

test_prefix = "_"
valid_event = {"text": test_prefix + "ud a valid Urban command"}
invalid_event = {"text": "this is missing a valid command prefix"}
urban_response = "Doubtless some pile of filth"

class TestUrbanEventHandler(unittest.TestCase):

    @patch.object(Config, "get_config_value")
    @patch.object(Config, "get_prefix", return_value=test_prefix)
    def test_handles_correct_event(self, prefix_method, config_method):
        handler = UrbanEventHandler()
        can_handle = handler.can_handle(valid_event)
        self.assertTrue(can_handle)

    @patch.object(Config, "get_config_value")
    @patch.object(Config, "get_prefix", return_value=test_prefix)
    def test_does_not_handle_different_event(self, prefix_method, config_method):
        handler = UrbanEventHandler()
        can_handle = handler.can_handle(invalid_event)
        self.assertFalse(can_handle)

    @patch.object(Config, "get_prefix", return_value=test_prefix)
    @patch.object(Config, "get_config_value")
    @patch.object(UrbanEventHandler, "_send_response")
    @patch.object(UrbanSearchCommand, "execute", return_value = urban_response)
    def test_command_execute_is_called(self, execute_method, response_method, config_method, prefix_method):
        handler = UrbanEventHandler()
        handler.handle(valid_event)
        execute_method.assert_called_once_with(valid_event["text"][4:])

    @patch.object(Config, "get_prefix", return_value=test_prefix)
    @patch.object(Config, "get_config_value")
    @patch.object(UrbanSearchCommand, "execute", return_value=urban_response)
    @patch.object(UrbanEventHandler, "_send_response")
    def test_command_result_is_correctly_built(self, response_method, execute_method, config_method, prefix_method):
        handler = UrbanEventHandler()
        handler.handle(valid_event)
        response_method.assert_called_once_with(urban_response, valid_event)

    if __name__ == '__main__':
        unittest.main()