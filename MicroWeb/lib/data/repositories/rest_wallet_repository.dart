import '../models/requests/create_account_request.dart';
import '../models/responses/wallet_response.dart';
import '../models/responses/ledger_response.dart';
import '../models/top_up_request.dart';
import '../models/responses/top_up_applied_response.dart';
import '../models/responses/top_up_idempotent_response.dart';
import '../services/api_client.dart';
import 'wallet_repository.dart';

/// Wallet/Accounts service repository calling endpoints via the API Gateway base URL.
class RestWalletRepository implements WalletRepository {
  RestWalletRepository(this._api);
  final ApiClient _api;

  // Accounts
  @override
  Future<dynamic> listAccounts() async {
    return await _api.get('/accounts');
  }

  @override
  Future<dynamic> getAccountById(String id) async {
    return await _api.get('/accounts/$id');
  }

  @override
  Future<dynamic> createAccount(CreateAccountRequest request) async {
    return await _api.post('/accounts', body: request.toJson());
  }

  @override
  Future<void> deleteAccount(String id) async {
    await _api.delete('/accounts/$id');
  }

  // Info / actions
  @override
  Future<dynamic> getBalance(String id) async {
    return await _api.get('/accounts/$id/balance');
  }

  @override
  Future<void> deposit(String id, Map<String, dynamic> requestBody) async {
    await _api.post('/accounts/$id/deposit', body: requestBody);
  }

  @override
  Future<void> withdraw(String id, Map<String, dynamic> requestBody) async {
    await _api.post('/accounts/$id/withdraw', body: requestBody);
  }

  @override
  Future<dynamic> listTransactions(String id, {Map<String, dynamic>? query}) async {
    return await _api.get('/accounts/$id/transactions', query: query);
  }

  // Wallets
  @override
  Future<WalletResponse> getWalletByUserId(String userId) async {
    final data = await _api.get('/wallets/$userId');
    return WalletResponse.fromJson(Map<String, dynamic>.from(data as Map));
  }

  @override
  Future<List<LedgerResponse>> getLedgerByUserId(String userId) async {
    final data = await _api.get('/wallets/$userId/ledger');
    final list = List<Map<String, dynamic>>.from(data as List);
    return list.map(LedgerResponse.fromJson).toList();
  }

  @override
  Future<dynamic> topUp(String userId, TopUpRequest request) async {
    final data = await _api.post('/wallets/$userId/topup', body: request.toJson());
    // Try to parse into known response models, otherwise return raw
    if (data is Map<String, dynamic>) {
      final map = Map<String, dynamic>.from(data);
      if (map.containsKey('balanceMinor')) {
        return TopUpAppliedResponse.fromJson(map);
      }
      if (map.containsKey('correlationId')) {
        return TopUpIdempotentResponse.fromJson(map);
      }
    }
    return data;
  }
}
