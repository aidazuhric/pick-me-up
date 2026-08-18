import 'package:flutter/material.dart';
import 'package:flutter_map/flutter_map.dart';
import 'package:latlong2/latlong.dart';

import '../../core/theme/app_colors.dart';
import '../../core/theme/app_spacing.dart';
import '../../models/order.dart';
import '../../models/taxi_company.dart';
import '../../providers/order_provider.dart';
import '../../services/geocoding_service.dart';
import '../../utils/api_error_handler.dart';
import '../../utils/formats.dart';
import 'address_search_sheet.dart';
import 'company_picker_sheet.dart';

enum _TripPoint { pickup, destination }

class OrderTaxiScreen extends StatefulWidget {
  const OrderTaxiScreen({super.key});

  @override
  State<OrderTaxiScreen> createState() => _OrderTaxiScreenState();
}

class _OrderTaxiScreenState extends State<OrderTaxiScreen> {
  static const LatLng _fallbackCentre = LatLng(43.3438, 17.8078);
  static const String _tileUrl = 'https://tile.openstreetmap.org/{z}/{x}/{y}.png';
  static const String _userAgent = 'ba.fit.pickmeup_mobile';
  static const String _unnamedPoint = 'Pinned point on the map';

  final _mapController = MapController();
  final _geocoding = GeocodingService();
  final _orders = OrderProvider();

  LatLng? _pickup;
  LatLng? _destination;
  String? _pickupAddress;
  String? _destinationAddress;
  TaxiCompany? _company;
  _TripPoint? _picking;
  bool _isLocating = false;
  bool _isSubmitting = false;
  Order? _placed;

  @override
  void dispose() {
    _mapController.dispose();
    super.dispose();
  }

  double? get _distanceKm {
    final pickup = _pickup;
    final destination = _destination;

    if (pickup == null || destination == null) {
      return null;
    }

    final metres = const Distance().as(LengthUnit.Meter, pickup, destination);

    return double.parse((metres / 1000).toStringAsFixed(2));
  }

  double? get _fareEstimate {
    final company = _company;
    final distance = _distanceKm;

    if (company == null || distance == null) {
      return null;
    }

    final start = company.startingPrice ?? 0;
    final perKm = company.pricePerKm ?? 0;

    return start + (distance * perKm);
  }

  void _frameTrip() {
    final pickup = _pickup;
    final destination = _destination;

    if (pickup == null || destination == null) {
      return;
    }

    _mapController.fitCamera(
      CameraFit.bounds(
        bounds: LatLngBounds(pickup, destination),
        padding: const EdgeInsets.fromLTRB(50, 190, 50, 300),
      ),
    );
  }

  void _beginPicking(_TripPoint point) {
    setState(() => _picking = point);
  }

  Future<void> _handleMapTap(LatLng point) async {
    final target = _picking;

    if (target == null) {
      return;
    }

    setState(() {
      _picking = null;
      _isLocating = true;

      if (target == _TripPoint.pickup) {
        _pickup = point;
        _pickupAddress = null;
      } else {
        _destination = point;
        _destinationAddress = null;
      }
    });

    _frameTrip();

    String? label;

    try {
      label = await _geocoding.describe(point);
    } catch (_) {
      label = null;
    }

    if (!mounted) return;

    setState(() {
      _isLocating = false;

      if (target == _TripPoint.pickup) {
        _pickupAddress = label ?? _unnamedPoint;
      } else {
        _destinationAddress = label ?? _unnamedPoint;
      }
    });
  }

  Future<void> _searchAddress(_TripPoint target) async {
    final place = await AddressSearchSheet.show(
      context,
      title: target == _TripPoint.pickup
          ? 'Where should we pick you up?'
          : 'Where are you going?',
      near: _pickup ?? _fallbackCentre,
    );

    if (place == null || !mounted) {
      return;
    }

    setState(() {
      _picking = null;

      if (target == _TripPoint.pickup) {
        _pickup = place.point;
        _pickupAddress = place.label;
      } else {
        _destination = place.point;
        _destinationAddress = place.label;
      }
    });

    if (_pickup != null && _destination != null) {
      _frameTrip();
    } else {
      _mapController.move(place.point, 15);
    }
  }

  void _clear(_TripPoint target) {
    setState(() {
      if (target == _TripPoint.pickup) {
        _pickup = null;
        _pickupAddress = null;
      } else {
        _destination = null;
        _destinationAddress = null;
      }
    });
  }

  Future<void> _chooseCompany() async {
    final choice = await CompanyPickerSheet.show(
      context,
      selectedId: _company?.id,
    );

    if (choice == null || !mounted) {
      return;
    }

    setState(() => _company = choice.company);
  }

  Future<void> _submit() async {
    final pickup = _pickup;

    if (pickup == null) {
      return;
    }

    setState(() => _isSubmitting = true);

    try {
      final order = await _orders.insert({
        'taxiCompanyId': _company?.id,
        'pickupLat': pickup.latitude,
        'pickupLng': pickup.longitude,
        'pickupAddress': _pickupAddress ?? _unnamedPoint,
        'destinationLat': _destination?.latitude,
        'destinationLng': _destination?.longitude,
        'destinationAddress': _destination == null ? null : _destinationAddress,
        'distanceKm': _distanceKm,
      });

      if (!mounted) return;

      setState(() {
        _placed = order;
        _isSubmitting = false;
      });
    } catch (error) {
      if (!mounted) return;

      setState(() => _isSubmitting = false);
      ApiErrorHandler.surface(context, error);
    }
  }

  void _startOver() {
    setState(() {
      _placed = null;
      _pickup = null;
      _destination = null;
      _pickupAddress = null;
      _destinationAddress = null;
      _company = null;
      _picking = null;
    });
  }

  @override
  Widget build(BuildContext context) {
    final placed = _placed;

    return Stack(
      children: [
        _buildMap(),
        if (placed == null)
          Positioned(
            top: AppSpacing.md,
            left: AppSpacing.md,
            right: AppSpacing.md,
            child: _buildTripCard(),
          ),
        if (placed == null && _picking != null)
          Positioned(
            top: 150,
            left: AppSpacing.md,
            right: AppSpacing.md,
            child: _buildPickingBanner(),
          ),
        Positioned(
          left: AppSpacing.md,
          right: AppSpacing.md,
          bottom: AppSpacing.md,
          child: placed == null
              ? _buildOrderPanel()
              : _buildConfirmation(placed),
        ),
      ],
    );
  }

  Widget _buildMap() {
    final pickup = _pickup;
    final destination = _destination;

    return FlutterMap(
      mapController: _mapController,
      options: MapOptions(
        initialCenter: _fallbackCentre,
        initialZoom: 14,
        minZoom: 3,
        maxZoom: 18,
        onTap: (_, point) => _handleMapTap(point),
      ),
      children: [
        TileLayer(
          urlTemplate: _tileUrl,
          userAgentPackageName: _userAgent,
          maxZoom: 19,
        ),
        if (pickup != null && destination != null)
          PolylineLayer(
            polylines: [
              Polyline(
                points: [pickup, destination],
                strokeWidth: 4,
                color: AppColors.mobileHeader.withValues(alpha: 0.75),
              ),
            ],
          ),
        MarkerLayer(
          markers: [
            if (pickup != null)
              _marker(pickup, AppColors.toggleOn, Icons.my_location),
            if (destination != null)
              _marker(destination, AppColors.danger, Icons.flag),
          ],
        ),
      ],
    );
  }

  Marker _marker(LatLng point, Color colour, IconData icon) {
    return Marker(
      point: point,
      width: 38,
      height: 38,
      child: Container(
        decoration: BoxDecoration(
          color: colour,
          shape: BoxShape.circle,
          border: Border.all(color: Colors.white, width: 3),
          boxShadow: [
            BoxShadow(
              color: Colors.black.withValues(alpha: 0.25),
              blurRadius: 6,
              offset: const Offset(0, 2),
            ),
          ],
        ),
        child: Icon(icon, color: Colors.white, size: 18),
      ),
    );
  }

  Widget _buildTripCard() {
    return _Card(
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          _TripPointRow(
            colour: AppColors.toggleOn,
            hint: 'Enter your location',
            value: _pickupAddress,
            isBusy: _isLocating && _pickup != null && _pickupAddress == null,
            isActive: _picking == _TripPoint.pickup,
            onTap: () => _beginPicking(_TripPoint.pickup),
            onSearch: () => _searchAddress(_TripPoint.pickup),
            onClear: _pickup == null ? null : () => _clear(_TripPoint.pickup),
          ),
          const Divider(height: 1),
          _TripPointRow(
            colour: AppColors.danger,
            hint: 'Enter your destination',
            value: _destinationAddress,
            isBusy: _isLocating && _destination != null && _destinationAddress == null,
            isActive: _picking == _TripPoint.destination,
            onTap: () => _beginPicking(_TripPoint.destination),
            onSearch: () => _searchAddress(_TripPoint.destination),
            onClear:
                _destination == null ? null : () => _clear(_TripPoint.destination),
          ),
        ],
      ),
    );
  }

  Widget _buildPickingBanner() {
    final isPickup = _picking == _TripPoint.pickup;

    return _Card(
      background: AppColors.mobileHeader,
      child: Padding(
        padding: const EdgeInsets.symmetric(
          horizontal: AppSpacing.md,
          vertical: AppSpacing.sm,
        ),
        child: Row(
          children: [
            const Icon(Icons.touch_app_outlined, color: Colors.white, size: 20),
            const SizedBox(width: AppSpacing.sm),
            Expanded(
              child: Text(
                isPickup
                    ? 'Tap the map to set your pick-up'
                    : 'Tap the map to set your destination',
                style: const TextStyle(color: Colors.white, fontSize: 13),
              ),
            ),
            TextButton(
              onPressed: () => setState(() => _picking = null),
              child: const Text(
                'CANCEL',
                style: TextStyle(color: Colors.white, fontSize: 12),
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildOrderPanel() {
    final theme = Theme.of(context);
    final distance = _distanceKm;
    final estimate = _fareEstimate;
    final canOrder = _pickup != null && !_isSubmitting;

    return _Card(
      child: ConstrainedBox(
        constraints: BoxConstraints(
          maxHeight: MediaQuery.of(context).size.height * 0.42,
        ),
        child: SingleChildScrollView(
          padding: const EdgeInsets.all(AppSpacing.md),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              Row(
                children: [
                  Expanded(
                    child: Text('Your ride', style: theme.textTheme.titleMedium),
                  ),
                  if (distance != null)
                    _Chip(
                      icon: Icons.straighten,
                      label: Formats.distance(distance),
                    ),
                ],
              ),
              const SizedBox(height: AppSpacing.sm),
              InkWell(
                onTap: _isSubmitting ? null : _chooseCompany,
                borderRadius: BorderRadius.circular(AppSpacing.controlRadius),
                child: Padding(
                  padding: const EdgeInsets.symmetric(vertical: AppSpacing.sm),
                  child: Row(
                    children: [
                      Icon(
                        _company == null ? Icons.groups_outlined : Icons.local_taxi,
                        size: 20,
                        color: AppColors.mobileHeader,
                      ),
                      const SizedBox(width: AppSpacing.sm),
                      Expanded(
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text(
                              _company?.name ?? 'Any available driver',
                              style: theme.textTheme.bodyMedium,
                              maxLines: 1,
                              overflow: TextOverflow.ellipsis,
                            ),
                            Text(
                              _company == null
                                  ? 'General order — every company sees it'
                                  : 'Ordered from this company only',
                              style: theme.textTheme.bodySmall,
                              maxLines: 1,
                              overflow: TextOverflow.ellipsis,
                            ),
                          ],
                        ),
                      ),
                      const Icon(Icons.chevron_right, color: AppColors.textMuted),
                    ],
                  ),
                ),
              ),
              const Divider(height: AppSpacing.lg),
              Text(
                estimate != null
                    ? 'Estimated fare ${Formats.money(estimate)}'
                    : 'The company sets the final price when the ride ends.',
                style: theme.textTheme.bodyMedium,
              ),
              if (estimate != null)
                Padding(
                  padding: const EdgeInsets.only(top: AppSpacing.xs),
                  child: Text(
                    'Estimate from this company\'s price list over the straight-line '
                    'distance. The server prices the finished ride.',
                    style: theme.textTheme.bodySmall,
                  ),
                ),
              const SizedBox(height: AppSpacing.md),
              FilledButton(
                onPressed: canOrder ? _submit : null,
                child: _isSubmitting
                    ? const SizedBox(
                        height: 20,
                        width: 20,
                        child: CircularProgressIndicator(
                          strokeWidth: 2,
                          color: Colors.white,
                        ),
                      )
                    : const Text('ORDER TAXI'),
              ),
              if (_pickup == null)
                Padding(
                  padding: const EdgeInsets.only(top: AppSpacing.sm),
                  child: Text(
                    'Set your pick-up point first — tap the field above, then the map.',
                    style: theme.textTheme.bodySmall,
                  ),
                ),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildConfirmation(Order order) {
    final theme = Theme.of(context);

    return _Card(
      child: ConstrainedBox(
        constraints: BoxConstraints(
          maxHeight: MediaQuery.of(context).size.height * 0.55,
        ),
        child: SingleChildScrollView(
          padding: const EdgeInsets.all(AppSpacing.md),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              Row(
                children: [
                  const Icon(Icons.check_circle, color: AppColors.toggleOn),
                  const SizedBox(width: AppSpacing.sm),
                  Expanded(
                    child: Text(
                      'Ride #${order.id} requested',
                      style: theme.textTheme.titleMedium,
                    ),
                  ),
                  _Chip(
                    icon: Icons.schedule,
                    label: order.status ?? '',
                  ),
                ],
              ),
              const SizedBox(height: AppSpacing.sm),
              Text(
                order.isGeneralOrder == true
                    ? 'Sent to every company. Waiting for a driver to accept.'
                    : 'Sent to ${order.companyName ?? 'the company'}. Waiting for a driver to accept.',
                style: theme.textTheme.bodyMedium,
              ),
              const Divider(height: AppSpacing.lg),
              _DetailRow(
                icon: Icons.my_location,
                colour: AppColors.toggleOn,
                text: order.pickupAddress ?? '',
              ),
              if ((order.destinationAddress ?? '').isNotEmpty)
                Padding(
                  padding: const EdgeInsets.only(top: AppSpacing.sm),
                  child: _DetailRow(
                    icon: Icons.flag,
                    colour: AppColors.danger,
                    text: order.destinationAddress!,
                  ),
                ),
              if (order.distanceKm != null)
                Padding(
                  padding: const EdgeInsets.only(top: AppSpacing.sm),
                  child: _DetailRow(
                    icon: Icons.straighten,
                    colour: AppColors.textMuted,
                    text: Formats.distance(order.distanceKm),
                  ),
                ),
              const SizedBox(height: AppSpacing.md),
              OutlinedButton(
                onPressed: _startOver,
                child: const Text('ORDER ANOTHER RIDE'),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class _Card extends StatelessWidget {
  const _Card({required this.child, this.background});

  final Widget child;
  final Color? background;

  @override
  Widget build(BuildContext context) {
    return Material(
      color: background ?? AppColors.surface,
      elevation: 4,
      borderRadius: BorderRadius.circular(AppSpacing.cardRadius),
      clipBehavior: Clip.antiAlias,
      child: child,
    );
  }
}

class _TripPointRow extends StatelessWidget {
  const _TripPointRow({
    required this.colour,
    required this.hint,
    required this.value,
    required this.isBusy,
    required this.isActive,
    required this.onTap,
    required this.onSearch,
    required this.onClear,
  });

  final Color colour;
  final String hint;
  final String? value;
  final bool isBusy;
  final bool isActive;
  final VoidCallback onTap;
  final VoidCallback onSearch;
  final VoidCallback? onClear;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final label = isBusy ? 'Looking up the address…' : value;

    return InkWell(
      onTap: onTap,
      child: Container(
        color: isActive ? AppColors.sectionTint : null,
        padding: const EdgeInsets.symmetric(
          horizontal: AppSpacing.md,
          vertical: AppSpacing.sm,
        ),
        child: Row(
          children: [
            Container(
              width: 12,
              height: 12,
              decoration: BoxDecoration(color: colour, shape: BoxShape.circle),
            ),
            const SizedBox(width: AppSpacing.md),
            Expanded(
              child: Text(
                label ?? hint,
                maxLines: 2,
                overflow: TextOverflow.ellipsis,
                style: label == null
                    ? theme.textTheme.bodyMedium?.copyWith(
                        color: AppColors.textMuted,
                      )
                    : theme.textTheme.bodyMedium,
              ),
            ),
            if (onClear != null)
              IconButton(
                tooltip: 'Clear',
                icon: const Icon(Icons.close, size: 18),
                onPressed: onClear,
              ),
            IconButton(
              tooltip: 'Search an address',
              icon: const Icon(Icons.search, size: 20),
              onPressed: onSearch,
            ),
          ],
        ),
      ),
    );
  }
}

class _Chip extends StatelessWidget {
  const _Chip({required this.icon, required this.label});

  final IconData icon;
  final String label;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 5),
      decoration: BoxDecoration(
        color: AppColors.sectionTint,
        borderRadius: BorderRadius.circular(AppSpacing.controlRadius),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(icon, size: 14, color: AppColors.mobileHeader),
          const SizedBox(width: 6),
          Text(
            label,
            style: const TextStyle(
              fontSize: 12,
              fontWeight: FontWeight.w600,
              color: AppColors.textPrimary,
            ),
          ),
        ],
      ),
    );
  }
}

class _DetailRow extends StatelessWidget {
  const _DetailRow({
    required this.icon,
    required this.colour,
    required this.text,
  });

  final IconData icon;
  final Color colour;
  final String text;

  @override
  Widget build(BuildContext context) {
    return Row(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Icon(icon, size: 16, color: colour),
        const SizedBox(width: AppSpacing.sm),
        Expanded(
          child: Text(
            text,
            style: Theme.of(context).textTheme.bodySmall,
            maxLines: 3,
            overflow: TextOverflow.ellipsis,
          ),
        ),
      ],
    );
  }
}
