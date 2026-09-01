import 'dart:math' as math;

import '../models/city.dart';
import '../providers/city_provider.dart';

class CityLocator {
  static const double _earthRadiusKm = 6371.0088;
  static const double fallbackLatitude = 43.3438;
  static const double fallbackLongitude = 17.8078;

  static final CityProvider _provider = CityProvider();
  static List<City> _cities = const <City>[];
  static City? _current;

  static City? get current => _current;

  static Future<void> ensureLoaded() async {
    if (_cities.isNotEmpty) return;

    final result = await _provider.get(
      filter: {'IsActive': true, 'Page': 1, 'PageSize': 100, 'SortBy': 'Name'},
    );

    _cities = (result.items ?? const <City>[])
        .where((city) => city.latitude != null && city.longitude != null)
        .toList();
  }

  static City? nearestTo(double latitude, double longitude) {
    if (_cities.isEmpty) return null;

    City? nearest;
    var shortest = double.infinity;

    for (final city in _cities) {
      final distance = _kilometresBetween(
        latitude,
        longitude,
        city.latitude!,
        city.longitude!,
      );

      if (distance < shortest) {
        shortest = distance;
        nearest = city;
      }
    }

    return nearest;
  }

  static Future<City?> resolveFor(double latitude, double longitude) async {
    await ensureLoaded();
    final city = nearestTo(latitude, longitude);

    if (city != null) _current = city;

    return city;
  }

  static Future<City?> resolveDefault() async {
    if (_current != null) return _current;

    return resolveFor(fallbackLatitude, fallbackLongitude);
  }

  static double _kilometresBetween(
    double fromLat,
    double fromLng,
    double toLat,
    double toLng,
  ) {
    final lat1 = _toRadians(fromLat);
    final lat2 = _toRadians(toLat);
    final deltaLat = _toRadians(toLat - fromLat);
    final deltaLng = _toRadians(toLng - fromLng);

    final h =
        math.sin(deltaLat / 2) * math.sin(deltaLat / 2) +
        math.cos(lat1) *
            math.cos(lat2) *
            math.sin(deltaLng / 2) *
            math.sin(deltaLng / 2);

    return 2 * _earthRadiusKm * math.asin(math.min(1.0, math.sqrt(h)));
  }

  static double _toRadians(double degrees) => degrees * math.pi / 180.0;
}
