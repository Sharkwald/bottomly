import unittest

from datetime import datetime

from commands.add_karma import AddKarmaCommand
from config import Config
from model.karma import Karma, KarmaType


class TestAddKarma(unittest.TestCase):

    def test_persistence(self):
        # Arrange
        Config().connect_to_db()
        awarded_to = "testUser1"
        awarded_by = "testUser2"
        test_reason = "testReason"
        test_type = KarmaType.POZZYPOZ
        k = Karma(awarded_to_username=awarded_to,
                  reason=test_reason,
                  awarded_by_username=awarded_by,
                  awarded=datetime.now(),
                  karma_type=str(test_type))

        # Act
        c = AddKarmaCommand()
        saved_karma = c.execute(k.awarded_to_username, k.awarded_by_username, k.reason, k.karma_type)

        # Assert
        self.assertIsNotNone(saved_karma._id)
        self.assertEqual(k.awarded_by_username, saved_karma.awarded_by_username)
        self.assertEqual(k.reason, saved_karma.reason)
        self.assertEqual(k.awarded_to_username, saved_karma.awarded_to_username)
        self.assertEqual(k.karma_type, saved_karma.karma_type)
        self.assertIsNotNone(saved_karma.awarded)

        # Tear down
        saved_karma.delete()

