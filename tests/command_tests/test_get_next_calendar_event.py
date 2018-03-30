import unittest

from commands.get_next_calendar_event import GetNextCalendarEventCommand


class TestGetNextCalendarEventCommand(unittest.TestCase):

    def test_spike(self):
        command = GetNextCalendarEventCommand()
        result = command.execute('OWlsN3JxOXVqYWZiYjBzbDVxcDg0cnJ1MXNAZ3JvdXAuY2FsZW5kYXIuZ29vZ2xlLmNvbQ')

        self.assertIsNotNone(result)


if __name__ == '__main__':
    unittest.main()
