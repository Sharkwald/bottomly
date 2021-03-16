import unittest
from unittest.mock import patch

from model.member import Member
from slack_channel.slack_parser import SlackParser

parser = SlackParser()

class TestSlackParser(unittest.TestCase):

    def test_single_word_no_slack_id(self):
        expected = "hello"
        actual = parser.replace_slack_id_tokens_with_usernames(expected)

        self.assertEqual(expected, actual)

    def test_non_slack_id_not_replaced(self):
        expected = "this text has no slack ids"
        actual = parser.replace_slack_id_tokens_with_usernames(expected)

        self.assertEqual(expected, actual)

    def test_slack_id_is_replaced(self):
        slack_id = 'U12345'
        slack_token = '<@' + slack_id + '>'
        expected_member = Member(username="test_username")

        with patch.object(Member, "get_member_by_slack_id", return_value=expected_member) as test_method:
            actual = parser.replace_slack_id_tokens_with_usernames(slack_token)

            self.assertEqual(expected_member.username, actual)
            test_method.assert_called_once_with(slack_id)

    def test_slack_ids_replaced_in_long_message(self):
        slack_id = 'U12345'
        expected_member = Member(username="test_username")

        message = "Hello this a longer message with a <@U12345> slack id in it."
        expected = "Hello this a longer message with a test_username slack id in it."

        with patch.object(Member, "get_member_by_slack_id", return_value=expected_member) as test_method:
            actual = parser.replace_slack_id_tokens_with_usernames(message)

            self.assertEqual(expected, actual)
            test_method.assert_called_once_with(slack_id)


    if __name__ == '__main__':
        unittest.main()