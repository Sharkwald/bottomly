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

    def test_valid_input_upper_case(self):
        command = RegSearchCommand()
        result = command.execute('7 YO')

        self.assertEqual(result, 'Yellow Mclaren p1 Unknown (January 2015) https://www.vehiclecheck.co.uk/VehicleCheck/ShowImage/?id=&CapType=0')

    def test_valid_input_i_to_1(self):
        command = RegSearchCommand()
        result = command.execute('i reg')

        self.assertEqual(result, 'Black Mclaren 720S V8 S-A (September 2018) https://www.vehiclecheck.co.uk/VehicleCheck/ShowImage/?id=5BCE18EBB6DE1598&CapType=0')

    def test_failed_lookup(self):
        command = RegSearchCommand()
        result = command.execute('BMT 216A')

        self.assertEqual(result, 'Sorry, we didn\'t recognise that registration.')

    def test_successful_lookup_rk66ro(self):
        command = RegSearchCommand()
        result = command.execute('rk 66kro')

        self.assertEqual(result, 'Yellow Ferrari Laferrari Ad S-A (January 2017) https://www.vehiclecheck.co.uk/VehicleCheck/ShowImage/?id=&CapType=0')

    def test_successful_lookup_nothing_found(self):
        command = RegSearchCommand()
        result = command.execute('f1')

        self.assertEqual(result, 'Sorry, we didn\'t recognise that registration.')

    def test_successful_lookup_missing_info(self):
        command = RegSearchCommand()
        result = command.execute('LF68 GXY')

        self.assertEqual(result, 'Sorry, we do not have enough information on this vehicle.')

if __name__ == '__main__':
    unittest.main()
