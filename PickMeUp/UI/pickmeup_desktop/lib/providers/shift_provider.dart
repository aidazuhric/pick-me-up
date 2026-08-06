import '../models/shift.dart';
import 'base_provider.dart';

class ShiftProvider extends BaseProvider<Shift> {
  ShiftProvider() : super('Shifts');

  @override
  Shift fromJson(dynamic data) => Shift.fromJson(data as Map<String, dynamic>);
}
