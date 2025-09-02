import '../models/item.dart';
import '../models/create_payment_request.dart';
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
    final id = (j['id'] ?? '').toString();
    final beneficiaryName = (j['beneficiaryName'] ?? '').toString();
    final beneficiaryAccount = (j['beneficiaryAccount'] ?? '').toString();
    final fromAccount = (j['fromAccount'] ?? '').toString();

    final amount = (j['amount'] as num?)?.toDouble() ?? 0.0;
    final currency = (j['currency'] ?? '').toString(); // enum: name or int -> toString() is fine
    final status = (j['status'] ?? '').toString(); // enum -> toString()

    final createdAtRaw = (j['createdAt'] ?? '').toString();
    final createdAt = DateTime.tryParse(createdAtRaw);

    // Infer direction: if it’s from my account -> debit (−), else credit (+)
    final isCredit = true; // !_sameIban(fromAccount, primaryAccountIban);
    final signedAmount = isCredit ? amount : -amount;

    return Item(
      id: id,
      title: beneficiaryName, // counterparty
      subtitle: (j['details'] ?? '').toString(),
      description: 'From: $fromAccount', // optional extra
      amount: signedAmount,
      currency: currency,
      credit: isCredit,
      status: status,
      account: beneficiaryAccount, // yellow chip in UI
      date: createdAt,
    );
  }

  bool _sameIban(String a, String b) => a.replaceAll(' ', '').toUpperCase() == b.replaceAll(' ', '').toUpperCase();
}
