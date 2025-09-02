import '../models/create_account_request.dart';
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
}
