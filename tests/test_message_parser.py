# coding=utf-8
import unittest
import json


class TestConfig(unittest.TestCase):
    def test_can_parse_google_command_event(self):
        event = '{"text": "_g a google test", "ts": "1521228743.000113", "channel": "bottomlytest", "team": "T3URW2LEB", "type": "message", "user": "owen", "source_team": "T3URW2LEB"}'
        parsed_event = json.loads(event)
        self.assertTrue(parsed_event["text"].startswith("_g"))

if __name__ == '__main__':
    unittest.main()