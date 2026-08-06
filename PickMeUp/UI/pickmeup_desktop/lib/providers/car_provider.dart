import '../models/car.dart';
import 'base_provider.dart';

class CarProvider extends BaseProvider<Car> {
  CarProvider() : super('Cars');

  @override
  Car fromJson(dynamic data) => Car.fromJson(data as Map<String, dynamic>);
}
