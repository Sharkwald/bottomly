# coding=utf-8
import unittest

from unittest.mock import patch

from slack_channel import AddKarmaReactionHandler
from commands import AddKarmaCommand
from model.karma import KarmaType

valid_event = {'reactor': 'kinduser', 'reaction': 'joy', 'reactee': 'deservinguser'}
valid_event_with_skintone = {'reactor': 'kinduser', 'reaction': '+1::skin-tone-2', 'reactee': 'deservinguser'}
invalid_event = {'reactor': 'kinduser', 'reaction': 'robot_face', 'reactee': 'deservinguser'}

class TestAddKarmaReactionHandler(unittest.TestCase):
    
    def test_parses_underlying_reaction_when_skintone_applied(self):
        handler = AddKarmaReactionHandler()
        raw_reaction = '+1::skin-tone-2'
        reaction = handler.parse_reaction(raw_reaction)
        self.assertEqual(reaction, '+1')

    def test_handles_valid_event(self):
        handler = AddKarmaReactionHandler()
        can_handle = handler.can_handle(valid_event)
        self.assertTrue(can_handle)

    def test_handles_valid_event_with_skintone(self):
        handler = AddKarmaReactionHandler()
        can_handle = handler.can_handle(valid_event_with_skintone)
        self.assertTrue(can_handle)

    def test_does_not_handle_different_event(self):
        handler = AddKarmaReactionHandler()
        can_handle = handler.can_handle(invalid_event)
        self.assertFalse(can_handle)

    @patch.object(AddKarmaReactionHandler, "_send_reaction_response")
    @patch.object(AddKarmaCommand, "execute")
    def test_command_executes_and_response_sent(self, execute_method, response_method):
        handler = AddKarmaReactionHandler()
        handler.handle(valid_event)
        execute_method.assert_called_once_with(
            awarded_to=valid_event["reactee"],
            awarded_by=valid_event["reactor"],
            reason="Reacted with "+valid_event["reaction"],
            karma_type=KarmaType.POZZYPOZ
        )
        response_method.assert_called_once_with(valid_event)