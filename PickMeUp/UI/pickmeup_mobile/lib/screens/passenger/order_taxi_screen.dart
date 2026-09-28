import 'dart:math' as math;

import 'package:flutter/material.dart';
import 'package:flutter_map/flutter_map.dart';
import 'package:latlong2/latlong.dart';

import '../../core/theme/app_colors.dart';
import '../../core/theme/app_spacing.dart';
import '../../models/order.dart';
import '../../models/order_quote.dart';
import '../../models/taxi_company.dart';
import '../../providers/order_provider.dart';
import '../../providers/payment_provider.dart';
import '../../services/city_locator.dart';
import '../../services/ride_payment_flow.dart';
import '../../services/geocoding_service.dart';
import '../../utils/api_error_handler.dart';
import '../../utils/formats.dart';
import '../../widgets/app_messages.dart';
import 'address_search_sheet.dart';
import 'company_picker_sheet.dart';
import '../../widgets/map_attribution.dart';

enum _TripPoint { pickup, destination }

class OrderTaxiScreen extends StatefulWidget {
  const OrderTaxiScreen({super.key, this.initialCompany});

  final TaxiCompany? initialCompany;

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
  final _payments = PaymentProvider();

  LatLng? _pickup;
  LatLng? _destination;
  String? _pickupAddress;
  String? _destinationAddress;
  TaxiCompany? _company;
  _TripPoint? _picking;
  bool _isLocating = false;
  bool _isSubmitting = false;
  Order? _placed;
  OrderQuote? _quote;
  bool _isQuoting = false;
  int _quoteToken = 0;
  String? _gatewayNotice;
  bool _isPaid = false;

  @override
  void initState() {
    super.initState();
    _company = widget.initialCompany;
    _prepareGateway();
  }

  Future<void> _prepareGateway() async {
    try {
      final notice = await RidePaymentFlow.prepareGateway(_payments);
      if (!mounted) return;
      setState(() => _gatewayNotice = notice);
    } catch (_) {
      if (!mounted) return;
      setState(() => _gatewayNotice = null);
    }
  }

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

  Map<String, dynamic>? _tripPayload() {
    final pickup = _pickup;

    if (pickup == null) {
      return null;
    }

    return {
      'taxiCompanyId': _company?.id,
      'pickupLat': pickup.latitude,
      'pickupLng': pickup.longitude,
      'pickupAddress': _pickupAddress ?? _unnamedPoint,
      'destinationLat': _destination?.latitude,
      'destinationLng': _destination?.longitude,
      'destinationAddress': _destination == null ? null : _destinationAddress,
    };
  }

  Future<void> _refreshQuote() async {
    final trip = _tripPayload();

    if (trip == null) {
      setState(() {
        _quote = null;
        _isQuoting = false;
      });
      return;
    }

    final token = ++_quoteToken;
    setState(() => _isQuoting = true);

    try {
      final quote = await _orders.quote(trip);

      if (!mounted || token != _quoteToken) return;

      setState(() {
        _quote = quote;
        _isQuoting = false;
      });
    } catch (_) {
      if (!mounted || token != _quoteToken) return;

      setState(() {
        _quote = null;
        _isQuoting = false;
      });
    }
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
        _rememberPickupCity(point);
      } else {
        _destination = point;
        _destinationAddress = null;
      }
    });

    _frameTrip();
    _refreshQuote();

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
        _rememberPickupCity(place.point);
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

    _refreshQuote();
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

    _refreshQuote();
  }

  int? _cityId;

  void _rememberPickupCity(LatLng point) {
    CityLocator.resolveFor(point.latitude, point.longitude).then((city) {
      if (!mounted || city?.id == null) return;

      if (city!.id != _cityId) {
        setState(() {
          _cityId = city.id;
          _company = null;
        });

        _refreshQuote();
      }
    });
  }

  Future<void> _chooseCompany() async {
    final choice = await CompanyPickerSheet.show(
      context,
      selectedId: _company?.id,
      cityId: _cityId,
    );

    if (choice == null || !mounted) {
      return;
    }

    setState(() => _company = choice.company);
    _refreshQuote();
  }

  Future<void> _submit() async {
    final pickup = _pickup;

    if (pickup == null) {
      return;
    }

    setState(() => _isSubmitting = true);

    Order order;

    try {
      order = await _orders.insert(_tripPayload()!);
    } catch (error) {
      if (!mounted) return;

      setState(() => _isSubmitting = false);
      ApiErrorHandler.surface(context, error);
      return;
    }

    if (!mounted) return;

    setState(() {
      _placed = order;
      _isPaid = false;
    });

    await _payForPlacedRide();
  }

  Future<void> _payForPlacedRide() async {
    final order = _placed;
    final orderId = order?.id;

    if (orderId == null) {
      setState(() => _isSubmitting = false);
      return;
    }

    if (_gatewayNotice != null) {
      setState(() => _isSubmitting = false);
      AppMessages.failure(context, _gatewayNotice!);
      return;
    }

    setState(() => _isSubmitting = true);

    final result = await RidePaymentFlow.pay(_payments, orderId);

    if (!mounted) return;

    setState(() {
      _isSubmitting = false;
      _isPaid = result.isPaid;
    });

    if (result.isPaid) {
      AppMessages.success(context, result.message);
    } else {
      AppMessages.failure(context, result.message);
    }
  }

  void _startOver() {
    setState(() {
      _placed = null;
      _pickup = null;
      _destination = null;
      _pickupAddress = null;
      _destinationAddress = null;
      _company = widget.initialCompany;
      _picking = null;
      _quote = null;
      _isPaid = false;
    });
  }

  @override
  Widget build(BuildContext context) {
    final placed = _placed;

    return LayoutBuilder(
      builder: (context, constraints) {
        final isShort = constraints.maxHeight < _shortViewport;
        final panelOwnsTripCard = isShort && placed == null;
        final panelMax = _panelMaxHeight(
          constraints.maxHeight,
          placed != null,
          panelOwnsTripCard,
        );

        return Stack(
          children: [
            _buildMap(panelMax + AppSpacing.md),
            if (placed == null && !panelOwnsTripCard)
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
                  ? _buildOrderPanel(panelMax, panelOwnsTripCard)
                  : _buildConfirmation(placed, panelMax),
            ),
          ],
        );
      },
    );
  }

  Widget _buildMap(double bottomInset) {
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
        MapAttribution(bottomInset: bottomInset),
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


  static const double _shortViewport = 420.0;
  static const double _tripCardHeight = 156.0;

  static double _panelMaxHeight(
    double available,
    bool isConfirmation,
    bool ownsTripCard,
  ) {
    if (ownsTripCard) {
      return available - AppSpacing.md * 2;
    }

    final preferred = available * (isConfirmation ? 0.55 : 0.42);

    return math.min(preferred, available - _tripCardHeight - AppSpacing.md * 2);
  }

  Widget _buildOrderPanel(double maxHeight, bool includeTripCard) {
    final theme = Theme.of(context);
    final distance = _distanceKm;
    final quote = _quote;
    final canOrder = _pickup != null && !_isSubmitting && quote != null;

    return _Card(
      child: ConstrainedBox(
        constraints: BoxConstraints(maxHeight: maxHeight),
        child: SingleChildScrollView(
          padding: const EdgeInsets.all(AppSpacing.md),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              if (includeTripCard) ...[
                _buildTripCard(),
                const SizedBox(height: AppSpacing.sm),
              ],
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
              if (_isQuoting)
                Row(
                  children: [
                    const SizedBox(
                      height: 16,
                      width: 16,
                      child: CircularProgressIndicator(strokeWidth: 2),
                    ),
                    const SizedBox(width: AppSpacing.sm),
                    Text('Working out the price…',
                        style: theme.textTheme.bodyMedium),
                  ],
                )
              else if (quote != null) ...[
                Row(
                  crossAxisAlignment: CrossAxisAlignment.end,
                  children: [
                    Expanded(
                      child: Text('Total to pay',
                          style: theme.textTheme.bodyMedium),
                    ),
                    Text(
                      Formats.money(quote.fare),
                      style: theme.textTheme.titleLarge?.copyWith(
                        color: AppColors.mobileHeader,
                        fontWeight: FontWeight.w700,
                      ),
                    ),
                  ],
                ),
                Padding(
                  padding: const EdgeInsets.only(top: AppSpacing.xs),
                  child: Text(
                    quote.isGeneralOrder
                        ? 'Average price of the companies driving here. Paid now, '
                              'before a driver takes the ride.'
                        : '${quote.companyName ?? 'This company'}\'s price list. '
                              'Paid now, before a driver takes the ride.',
                    style: theme.textTheme.bodySmall,
                  ),
                ),
              ] else
                Text(
                  'Set your pick-up and destination to see the price.',
                  style: theme.textTheme.bodyMedium,
                ),
              if (_gatewayNotice != null)
                Padding(
                  padding: const EdgeInsets.only(top: AppSpacing.sm),
                  child: Text(
                    _gatewayNotice!,
                    style: theme.textTheme.bodySmall?.copyWith(
                      color: AppColors.danger,
                    ),
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
                    : Text(quote == null
                        ? 'ORDER TAXI'
                        : 'PAY ${Formats.money(quote.fare)} AND ORDER'),
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

  Widget _buildConfirmation(Order order, double maxHeight) {
    final theme = Theme.of(context);

    return _Card(
      child: ConstrainedBox(
        constraints: BoxConstraints(maxHeight: maxHeight),
        child: SingleChildScrollView(
          padding: const EdgeInsets.all(AppSpacing.md),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              Row(
                children: [
                  Icon(
                    _isPaid ? Icons.check_circle : Icons.payments_outlined,
                    color: _isPaid ? AppColors.toggleOn : AppColors.danger,
                  ),
                  const SizedBox(width: AppSpacing.sm),
                  Expanded(
                    child: Text(
                      _isPaid ? 'Ride #${order.id} paid' : 'Ride #${order.id} not paid yet',
                      style: theme.textTheme.titleMedium,
                    ),
                  ),
                  if (order.fare != null)
                    _Chip(
                      icon: Icons.payments,
                      label: Formats.money(order.fare!),
                    ),
                ],
              ),
              const SizedBox(height: AppSpacing.sm),
              Text(
                _isPaid
                    ? (order.isGeneralOrder == true
                          ? 'Paid — waiting for a driver to accept. Every company driving here can see it.'
                          : 'Paid — waiting for a driver from ${order.companyName ?? 'the company'} to accept.')
                    : 'No driver sees this ride until it is paid.',
                style: theme.textTheme.bodyMedium,
              ),
              if (!_isPaid)
                Padding(
                  padding: const EdgeInsets.only(top: AppSpacing.sm),
                  child: FilledButton.icon(
                    onPressed: _isSubmitting ? null : _payForPlacedRide,
                    icon: const Icon(Icons.credit_card, size: 18),
                    label: Text(
                      order.fare == null
                          ? 'PAY NOW'
                          : 'PAY ${Formats.money(order.fare!)} NOW',
                    ),
                  ),
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
