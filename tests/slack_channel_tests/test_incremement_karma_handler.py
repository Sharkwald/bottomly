
import unittest
from unittest.mock import patch

from model.karma import KarmaType
from slack_channel import IncrementKarmaEventHandler


class TestIncrementKarmaHandler(unittest.TestCase):

    handler = IncrementKarmaEventHandler()

    def _check_parsing(self, command_message, expected):
        # Act
        parsed_command = self.handler._parse_command_text(command_message)

        # Assert
        self.assertEqual(expected, parsed_command)

    def test_reasonless_command_one_word_recipient(self):
        # Arrange
        command_message = "++recipient"
        expected = {"recipient": "recipient", "reason":"", "karma_type": KarmaType.POZZYPOZ}

        # Act & Assert
        self._check_parsing(command_message, expected)

    @patch.object(IncrementKarmaEventHandler, "_username_is_known", return_value=False)
    def test_reasonless_command_multi_word_recipient(self, known_user_method):
        # Arrange
        command_message = "++some recipient"
        expected = {"recipient": "some recipient", "reason": "", "karma_type": KarmaType.POZZYPOZ}

        # Act & Assert
        self._check_parsing(command_message, expected)

    def test_reasoned_command_one_word_recipient(self):
        # Arrange
        command_message = "++recipient for some reason"
        expected = {"recipient": "recipient", "reason": "some reason", "karma_type": KarmaType.POZZYPOZ}

        # Act & Assert
        self._check_parsing(command_message, expected)

    @patch.object(IncrementKarmaEventHandler, "_username_is_known", return_value=False)
    def test_reasoned_command_multi_word_recipient(self, known_user_method):
        # Arrange
        command_message = "++some recipient for some reason"
        expected = {"recipient": "some recipient", "reason": "some reason", "karma_type": KarmaType.POZZYPOZ}

        # Act & Assert
        self._check_parsing(command_message, expected)

    @patch.object(IncrementKarmaEventHandler, "_username_is_known", return_value=True)
    def test_reasoned_command_user_recipient_no_for(self, known_user_method):
        # Arrange
        command_message = "++username is sexy"
        expected = {"recipient": "username", "reason": "is sexy", "karma_type": KarmaType.POZZYPOZ}

        # Act & Assert
        self._check_parsing(command_message, expected)

    def test_reasoned_command_user_recipient_with_for(self):
        # Arrange
        command_message = "++username for being sexy"
        expected = {"recipient": "username", "reason": "being sexy", "karma_type": KarmaType.POZZYPOZ}

        # Act & Assert
        self._check_parsing(command_message, expected)

    def test_reasoned_command_user_recipient_with_multiple_fors(self):
        command_message = "++username for being sexy and for being nice"
        expected = {"recipient": "username", "reason": "being sexy and for being nice", "karma_type": KarmaType.POZZYPOZ}

        # Act & Assert
        self._check_parsing(command_message, expected)