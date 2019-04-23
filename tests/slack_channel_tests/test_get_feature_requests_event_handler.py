# coding=utf-8
import unittest
from unittest.mock import patch

import os

from commands import GetFeatureRequestsByStatusCommand
from config import Config
from model.feature_request import FeatureRequest, FeatureRequestState
from slack_channel import GetFeatureRequestsEventHandler


def create_request(requester, request) -> FeatureRequest:
    fr = FeatureRequest(requester=requester,
                        request=request,
                        request_state=str(FeatureRequestState.REQUESTED))
    return fr


test_prefix = "_"
valid_event_no_state = {"text": test_prefix + "requestedFeatures"}
valid_event_requested = {"text": test_prefix + "requestedFeatures requested"}
valid_event_delivered = {"text": test_prefix + "requestedFeatures delivered"}
valid_event_in_progress = {"text": test_prefix + "requestedFeatures in_progress"}
valid_event_rejected = {"text": test_prefix + "requestedFeatures rejected"}
valid_event_guff_state= {"text": test_prefix + "requestedFeatures wibble "}
invalid_event = {'text': '_g test'}
help_event = {"text": test_prefix + "requestedFeatures -?"}
command_result = list([create_request("someone", "do something"),
                       create_request("someone else", "do something else")])
expected_response = "Current feature requests with a state of requested:" + os.linesep + \
                    "\"do something\" from someone" + os.linesep + \
                    "\"do something else\" from someone else"


@patch.object(Config, "connect_to_db")
@patch.object(Config, "get_config_value")
@patch.object(Config, "get_prefix", return_value=test_prefix)
class TestGetFeatureRequestsEventHandler(unittest.TestCase):

    def test_handles_correct_event_no_size(self, prefix_method, config_method, db_method):
        handler = GetFeatureRequestsEventHandler()
        can_handle = handler.can_handle(valid_event_no_state)
        self.assertTrue(can_handle)

    def test_handles_correct_event_with_requested(self, prefix_method, config_method, db_method):
        handler = GetFeatureRequestsEventHandler()
        can_handle = handler.can_handle(valid_event_requested)
        self.assertTrue(can_handle)

    def test_handles_correct_event_with_delivered(self, prefix_method, config_method, db_method):
        handler = GetFeatureRequestsEventHandler()
        can_handle = handler.can_handle(valid_event_delivered)
        self.assertTrue(can_handle)

    def test_handles_correct_event_with_rejected(self, prefix_method, config_method, db_method):
        handler = GetFeatureRequestsEventHandler()
        can_handle = handler.can_handle(valid_event_rejected)
        self.assertTrue(can_handle)

    def test_handles_correct_event_with_in_progress(self, prefix_method, config_method, db_method):
        handler = GetFeatureRequestsEventHandler()
        can_handle = handler.can_handle(valid_event_in_progress)
        self.assertTrue(can_handle)

    def test_handles_correct_event_with_guff_state(self, prefix_method, config_method, db_method):
        handler = GetFeatureRequestsEventHandler()
        can_handle = handler.can_handle(valid_event_guff_state)
        self.assertTrue(can_handle)

    def test_does_not_handle_different_event(self, prefix_method, config_method, db_method):
        handler = GetFeatureRequestsEventHandler()
        can_handle = handler.can_handle(invalid_event)
        self.assertFalse(can_handle)

    @patch.object(GetFeatureRequestsEventHandler, "_send_message_response")
    @patch.object(GetFeatureRequestsByStatusCommand, "execute", return_value=command_result)
    def test_command_execute_is_called_with_no_state(self, execute_method, response_method, config_method,
                                                     prefix_method, db_method):
        handler = GetFeatureRequestsEventHandler()
        handler.handle(valid_event_no_state)
        execute_method.assert_called_once_with(FeatureRequestState.REQUESTED)

    @patch.object(GetFeatureRequestsEventHandler, "_send_message_response")
    @patch.object(GetFeatureRequestsByStatusCommand, "execute", return_value=command_result)
    def test_command_execute_is_called_with_requested(self, execute_method, response_method, config_method,
                                                      prefix_method, db_method):
        handler = GetFeatureRequestsEventHandler()
        handler.handle(valid_event_requested)
        execute_method.assert_called_once_with(FeatureRequestState.REQUESTED)

    @patch.object(GetFeatureRequestsEventHandler, "_send_message_response")
    @patch.object(GetFeatureRequestsByStatusCommand, "execute", return_value=command_result)
    def test_command_execute_is_called_with_delivered(self, execute_method, response_method, config_method,
                                                      prefix_method, db_method):
        handler = GetFeatureRequestsEventHandler()
        handler.handle(valid_event_delivered)
        execute_method.assert_called_once_with(FeatureRequestState.DELIVERED)

    @patch.object(GetFeatureRequestsEventHandler, "_send_message_response")
    @patch.object(GetFeatureRequestsByStatusCommand, "execute", return_value=command_result)
    def test_command_execute_is_called_with_in_progress(self, execute_method, response_method, config_method,
                                                        prefix_method, db_method):
        handler = GetFeatureRequestsEventHandler()
        handler.handle(valid_event_in_progress)
        execute_method.assert_called_once_with(FeatureRequestState.IN_PROGRESS)

    @patch.object(GetFeatureRequestsEventHandler, "_send_message_response")
    @patch.object(GetFeatureRequestsByStatusCommand, "execute", return_value=command_result)
    def test_command_execute_is_called_with_rejected(self, execute_method, response_method, config_method,
                                                     prefix_method, db_method):
        handler = GetFeatureRequestsEventHandler()
        handler.handle(valid_event_rejected)
        execute_method.assert_called_once_with(FeatureRequestState.REJECTED)

    @patch.object(GetFeatureRequestsEventHandler, "_send_message_response")
    @patch.object(GetFeatureRequestsByStatusCommand, "execute", return_value=command_result)
    def test_command_execute_is_not_called_with_guff_state(self, execute_method, response_method, config_method,
                                                           prefix_method, db_method):
        handler = GetFeatureRequestsEventHandler()
        handler.handle(valid_event_guff_state)
        execute_method.assert_not_called()
        response_method.assert_called_once_with("That's not a valid request state, try asking for help.",
                                                valid_event_guff_state)

    @patch.object(GetFeatureRequestsByStatusCommand, "execute", return_value=command_result)
    @patch.object(GetFeatureRequestsEventHandler, "_send_message_response")
    def test_command_result_is_correctly_built(self, response_method, execute_method, config_method,
                                               prefix_method, db_method):
        handler = GetFeatureRequestsEventHandler()
        handler.handle(valid_event_no_state)
        response_method.assert_called_once_with(expected_response, valid_event_no_state)

    @patch.object(GetFeatureRequestsByStatusCommand, "get_purpose", return_value="Requested Features")
    @patch.object(GetFeatureRequestsEventHandler, "_send_message_response")
    def test_get_usage(self, response_method, purpose_method, config_method, prefix_method, db_method):
        handler = GetFeatureRequestsEventHandler()
        handler.handle(help_event)
        expected_help = "Get requested features by status" + os.linesep + \
                        "Requested Features" + os.linesep + "Usage: `" + \
                        test_prefix + "requestedFeatures [requested|in_progress|delivered|rejected (default is " \
                                      "requested)]`"
        response_method.assert_called_once_with(expected_help, help_event)


if __name__ == '__main__':
    unittest.main()
