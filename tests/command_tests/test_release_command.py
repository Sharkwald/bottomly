import unittest

from commands.release import ReleaseCommand

class TestReleaseCommand(unittest.TestCase):
    def test_empty_input(self):
        command = ReleaseCommand()
        result = command.execute()
        self.assertIsNotNone(result)

if __name__ == '__main__':
    unittest.main()
