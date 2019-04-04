import unittest

from commands.reg_search import RegSearchCommand


class TestUrbanSearchCommand(unittest.TestCase):

    def test_empty_input(self):
        command = RegSearchCommand()
        result = command.execute('')

        self.assertEqual(result, 'Registration missing')

    def test_input_too_long(self):
        command = RegSearchCommand()
        result = command.execute('a random string long string')

        self.assertEqual(result, 'Registration too long.')

    def test_input_has_Special_Characters(self):
        command = RegSearchCommand()
        result = command.execute('Registration should not contain special characters')

        self.assertEqual(result, 'Registration too long.')

    def test_valid_input(self):
        command = RegSearchCommand()
        result = command.execute('a car reg')

        self.assertEqual(result, 'https://www.vehiclecheck.co.uk/?vrm=acarreg')

    def test_valid_input_upper_case(self):
        command = RegSearchCommand()
        result = command.execute('VR 00M')

        self.assertEqual(result, 'https://www.vehiclecheck.co.uk/?vrm=vr00m')

    def test_valid_input_i_to_1(self):
        command = RegSearchCommand()
        result = command.execute('ABC iIi')

        self.assertEqual(result, 'https://www.vehiclecheck.co.uk/?vrm=abc111')


if __name__ == '__main__':
    unittest.main()
