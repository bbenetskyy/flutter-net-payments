import '../models/item.dart';
import '../models/create_payment_request.dart';
import '../models/payment_dto.dart';
import '../services/api_client.dart';
import 'item_repository.dart';
import 'payments_repository.dart';

class RestPaymentsRepository implements ItemRepository, PaymentsRepository {
  RestPaymentsRepository(this._api);
  final ApiClient _api;

  // PaymentsRepository (raw access)
  @override
  Future<dynamic> listPayments({Map<String, dynamic>? query}) async {
    return await _api.get('/payments', query: query);
  }

  @override
  Future<dynamic> getPaymentById(String id) async {
    return await _api.get('/payments/$id');
  }

  @override
  Future<dynamic> createPayment(CreatePaymentRequest request) async {
    return await _api.post('/payments', body: request.toJson());
  }

  // ItemRepository (UI mapping)
  @override
  Future<List<Item>> fetchItems() async {
    // Adjust the route to your backend
    final data = await _api.get('/payments');

    if (data is List) {
      return data.map((e) => _mapPayment(Map<String, dynamic>.from(e))).toList();
    }

    // If your backend wraps with { items: [...] }
    if (data is Map && data['items'] is List) {
      final list = (data['items'] as List).map((e) => _mapPayment(Map<String, dynamic>.from(e))).toList();
      return list;
    }

    throw StateError('Unexpected response shape for /payments');
  }

  @override
  Future<Item> fetchItem(String id) async {
    final data = await _api.get('/payments/$id');
    return _mapPayment(Map<String, dynamic>.from(data));
  }

  // Not used by the transactions UI, keep as no-ops (or implement when you have endpoints)
  @override
  Future<Item> upsertItem(Item item) async => item;

  @override
  Future<void> deleteItem(String id) async {}

  // ---- mapping helpers ----

  Item _mapPayment(Map<String, dynamic> j) {
    final dto = PaymentDto.fromJson(j);
    return _mapPaymentDto(dto);
  }

  Item _mapPaymentDto(PaymentDto d) {
    final amount = d.amount ?? 0.0;
    // Infer direction: if it’s from my account -> debit (−), else credit (+)
    final isCredit = true; // replace with real check when primary account is known
    final signedAmount = isCredit ? amount : -amount;

    final createdAt = DateTime.tryParse(d.createdAt ?? '');

    return Item(
      id: d.id,
      title: (d.beneficiaryName ?? '').toString(),
      subtitle: (d.details ?? '').toString(),
      description: 'From: ${d.fromAccount ?? ''}',
      amount: signedAmount,
      currency: d.currency,
      credit: isCredit,
      status: d.status,
      account: d.beneficiaryAccount,
      date: createdAt,
    );
  }

  bool _sameIban(String a, String b) => a.replaceAll(' ', '').toUpperCase() == b.replaceAll(' ', '').toUpperCase();
}
