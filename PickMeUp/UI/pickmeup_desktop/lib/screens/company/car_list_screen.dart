import 'package:flutter/material.dart';

import '../../models/car.dart';
import '../../models/search_result.dart';
import '../../providers/car_provider.dart';
import '../../providers/reference_provider.dart';
import '../../utils/api_error_handler.dart';
import '../../widgets/app_messages.dart';
import '../../widgets/confirm_dialog.dart';
import '../../widgets/crud_data_table.dart';
import 'car_form.dart';

class CarListScreen extends StatefulWidget {
  const CarListScreen({super.key});

  @override
  State<CarListScreen> createState() => _CarListScreenState();
}

class _CarListScreenState extends State<CarListScreen> {
  final _provider = CarProvider();
  final _categoryProvider = ReferenceProvider('VehicleCategories');

  Future<SearchResult<Car>> _load(
    int page,
    int pageSize,
    String? search,
    String? sortBy,
  ) {
    return _provider.get(filter: {
      'Page': page,
      'PageSize': pageSize,
      'IncludeTotalCount': true,
      'SortBy': ?sortBy,
      'Plate': ?search,
    });
  }

  Future<bool> _create() => CarForm.show(
        context,
        provider: _provider,
        categoryProvider: _categoryProvider,
      );

  Future<bool> _edit(Car car) => CarForm.show(
        context,
        provider: _provider,
        categoryProvider: _categoryProvider,
        existing: car,
      );

  Future<bool> _delete(Car car) async {
    final confirmed = await ConfirmDialog.show(
      context,
      title: 'Remove ${car.description}?',
      message:
          '${car.plate} leaves the fleet. A vehicle still assigned to a driver cannot be removed — end its assignment first.',
      confirmLabel: 'DELETE',
      isDestructive: true,
    );

    if (!confirmed) return false;

    try {
      await _provider.remove(car.id!);
      if (!mounted) return false;
      AppMessages.success(context, '${car.plate} was removed from the fleet.');
      return true;
    } catch (e) {
      if (!mounted) return false;
      ApiErrorHandler.surface(context, e);
      return false;
    }
  }

  @override
  Widget build(BuildContext context) {
    return CrudDataTable<Car>(
      title: 'Vehicles',
      subtitle: 'The cars your company can put on the road.',
      searchHint: 'Search by registration plate',
      loadPage: _load,
      onCreate: _create,
      onEdit: _edit,
      onDelete: _delete,
      imageUrlOf: (car) => car.photoImageUrl,
      columns: [
        CrudColumn(label: 'Make', value: (c) => c.make ?? '', sortKey: 'Make'),
        CrudColumn(label: 'Model', value: (c) => c.model ?? '', sortKey: 'Model'),
        CrudColumn(label: 'Plate', value: (c) => c.plate ?? '', sortKey: 'Plate'),
        CrudColumn(label: 'Year', value: (c) => c.year?.toString() ?? '', sortKey: 'Year'),
        CrudColumn(label: 'Seats', value: (c) => c.seats?.toString() ?? '', sortKey: 'Seats'),
        CrudColumn(label: 'Category', value: (c) => c.categoryName ?? ''),
        CrudColumn(
          label: 'Status',
          value: (c) => c.isActive == true ? 'In service' : 'Off the road',
        ),
      ],
    );
  }
}
