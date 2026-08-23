import 'dart:convert';

import 'package:http/http.dart' as http;

import '../models/driver.dart';
import '../models/driver_location.dart';
import 'base_provider.dart';

class DriverProvider extends BaseProvider<Driver> {
  DriverProvider() : super('Drivers');

  @override
  Driver fromJson(dynamic data) => Driver.fromJson(data as Map<String, dynamic>);

  Future<Driver?> findByEmail(String email) async {
    final result = await get(filter: {'Page': 1, 'PageSize': 100});

    for (final driver in result.items ?? const <Driver>[]) {
      if (driver.email?.toLowerCase() == email.toLowerCase()) {
        return driver;
      }
    }

    return null;
  }

  Future<DriverLocation> reportLocation(double latitude, double longitude) async {
    final response = await http.post(
      Uri.parse('${BaseProvider.baseUrl}$endpoint/Me/Location'),
      headers: createHeaders(),
      body: jsonEncode({'latitude': latitude, 'longitude': longitude}),
    );
    validateResponse(response);

    return DriverLocation.fromJson(jsonDecode(response.body));
  }

  Future<Driver> resign(int driverId) async {
    final response = await http.post(
      Uri.parse('${BaseProvider.baseUrl}$endpoint/$driverId/Resign'),
      headers: createHeaders(),
    );
    validateResponse(response);

    return Driver.fromJson(jsonDecode(response.body));
  }
}
