# coding=utf-8
import unittest
from unittest.mock import patch

import os

from commands import CocktailOfTheWeekSearchCommand
from config import Config
from slack_channel import CocktailOfTheWeekEventHandler

test_prefix = "_"
valid_event = {"text": test_prefix + "cotw"}
invalid_event = {"text": "this is missing a valid command prefix"}
help_event = {"text": test_prefix + "cotw -?"}
cotw_response = "Water. (£0.00). Water"


@patch.object(Config, "get_config_value")
@patch.object(Config, "get_prefix", return_value=test_prefix)
class TestCocktailOfTheWeekEventHandler(unittest.TestCase):

    def test_handles_correct_event(self, prefix_method, config_method):
        handler = CocktailOfTheWeekEventHandler()
        can_handle = handler.can_handle(valid_event)
        self.assertTrue(can_handle)

    def test_does_not_handle_different_event(self, prefix_method, config_method):
        handler = CocktailOfTheWeekEventHandler()
        can_handle = handler.can_handle(invalid_event)
        self.assertFalse(can_handle)

    @patch.object(CocktailOfTheWeekEventHandler, "_send_message_response")
    @patch.object(CocktailOfTheWeekSearchCommand, "execute", return_value=cotw_response)
    def test_command_execute_is_called(self, execute_method, response_method, config_method, prefix_method):
        handler = CocktailOfTheWeekEventHandler()
        handler.handle(valid_event)
        execute_method.assert_called_once_with()

    @patch.object(CocktailOfTheWeekSearchCommand, "execute", return_value=cotw_response)
    @patch.object(CocktailOfTheWeekEventHandler, "_send_message_response")
    def test_command_result_is_correctly_built(self, response_method, execute_method, config_method, prefix_method):
        handler = CocktailOfTheWeekEventHandler()
        handler.handle(valid_event)
        response_method.assert_called_once_with(cotw_response, valid_event)

    @patch.object(CocktailOfTheWeekSearchCommand, "execute", return_value=None)
    @patch.object(CocktailOfTheWeekEventHandler, "_send_message_response")
    def test_no_result_message_correctly_sent(self, response_method, execute_method, config_method, prefix_method):
        handler = CocktailOfTheWeekEventHandler()
        handler.handle(valid_event)
        empty_results_message = "You're on your own I'm afraid - https://thebrassmonkeygla.co.uk/."
        response_method.assert_called_once_with(empty_results_message, valid_event)

    @patch.object(CocktailOfTheWeekSearchCommand, "get_purpose", return_value="Cocktail of the Week")
    @patch.object(CocktailOfTheWeekEventHandler, "_send_message_response")
    def test_get_usage(self, response_method, purpose_method, config_method, prefix_method):
        handler = CocktailOfTheWeekEventHandler()
        handler.handle(help_event)
        expected_help = "Cocktail of the week lookup from the Brass Monkey" + os.linesep + \
                        "Cocktail of the Week" + os.linesep + \
                        "Usage: `" + test_prefix + "cotw " + "`"
        purpose_method.assert_called_once_with()
        response_method.assert_called_once_with(expected_help, help_event)

    if __name__ == '__main__':
        unittest.main()