import unittest
from commands.reg_search import RegSearchCommand


class TestUrbanSearchCommand(unittest.TestCase):
    def test_empty_input(self):
        command = RegSearchCommand()
        result = command.execute('')

        self.assertIsNone(result)

    def test_garbage_input(self):
        command = RegSearchCommand()
        result = command.execute('asdhbhklasdvfbuioasvfbhuilasdvfbhjkl')

        self.assertEqual(result, "https://www.vehiclecheck.co.uk/?vrm=asdhbhklasdvfbuioasvfbhuilasdvfbhjkl")

    def test_valid_input(self):
        command = RegSearchCommand()
        result = command.execute("ACarReg")

        self.assertEqual(result, "https://www.vehiclecheck.co.uk/?vrm=ACarReg")


if __name__ == '__main__':
    unittest.main()
