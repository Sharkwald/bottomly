import unittest
from commands.urban_search import UrbanSearchCommand


class TestUrbanSearchCommand(unittest.TestCase):
    def test_empty_input(self):
        command = UrbanSearchCommand()
        result = command.execute('')

        self.assertIsNone(result)

    def test_garbage_input(self):
        command = UrbanSearchCommand()
        result = command.execute('asdhbhklasdvfbuioasvfbhuilasdvfbhjkl')

        self.assertIsNone(result)

    def test_valid_input(self):
        command = UrbanSearchCommand()
        result = command.execute("python")

        self.assertIsNotNone(result)

    def test_multi_word_search(self):
        command = UrbanSearchCommand()
        result = command.execute("hugh laurie")

        print(result)
        self.assertIsNotNone(result)


if __name__ == '__main__':
    unittest.main()
