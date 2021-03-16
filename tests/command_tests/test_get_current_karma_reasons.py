import unittest

from unittest.mock import patch

from commands import GetCurrentKarmaReasonsCommand
from model.karma import Karma


class TestGetCurrentNetKarmaCommand(unittest.TestCase):

    def test_get_net_karma_any_input(self):
        expected_reasons = {'reasonless': 10, 'reasoned': [Karma(), Karma()]}
        username = "test_username"
        with patch.object(Karma, "get_current_karma_reasons_for_recipient",
                          return_value=expected_reasons) as test_method:
            c = GetCurrentKarmaReasonsCommand()
            actual_reasons = c.execute(username)

            self.assertEqual(expected_reasons, actual_reasons)
            test_method.assert_called_once_with(username)


if __name__ == '__main__':
    unittest.main()