import '../models/city.dart';
import 'base_provider.dart';

class CityProvider extends BaseProvider<City> {
  CityProvider() : super('Cities');

  @override
  City fromJson(dynamic data) => City.fromJson(data as Map<String, dynamic>);
}
