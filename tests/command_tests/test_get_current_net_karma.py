import unittest

from unittest.mock import patch

from commands import GetCurrentNetKarmaCommand
from model.karma import Karma


class TestGetCurrentNetKarmaCommand(unittest.TestCase):

    def test_get_net_karma_any_input(self):
        expected_karma = 10
        username = "test_username"
        with patch.object(Karma, "get_current_net_karma_for_recipient", return_value=expected_karma) as test_method:
            c = GetCurrentNetKarmaCommand()
            actual_karma = c.execute(username)

            self.assertEqual(expected_karma, actual_karma)
            test_method.assert_called_once_with(username)


if __name__ == '__main__':
    unittest.main()