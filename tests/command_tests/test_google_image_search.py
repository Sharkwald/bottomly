import unittest
from commands.google_image_search import GoogleImageSearchCommand


class TestGoogleImageSearchCommand(unittest.TestCase):
    def test_empty_input(self):
        command = GoogleImageSearchCommand()
        result = command.execute('')

        self.assertIsNone(result)

    def test_garbage_input(self):
        command = GoogleImageSearchCommand()
        result = command.execute('asdhbhklasdvfbuioasvfbhuilasdvfbhjkl')

        self.assertIsNone(result)

    def test_valid_input(self):
        command = GoogleImageSearchCommand()
        result = command.execute("doctor who")
        self.assertIsNotNone(result)
        
if __name__ == '__main__':
    unittest.main()