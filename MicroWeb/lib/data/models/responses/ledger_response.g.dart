// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'ledger_response.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

LedgerResponse _$LedgerResponseFromJson(Map<String, dynamic> json) =>
    $checkedCreate('LedgerResponse', json, ($checkedConvert) {
      final val = LedgerResponse(
        id: $checkedConvert('id', (v) => v as String?),
        walletId: $checkedConvert('walletId', (v) => v as String?),
        amountMinor: $checkedConvert(
          'amountMinor',
          (v) => (v as num?)?.toInt(),
        ),
        currency: $checkedConvert(
          'currency',
          (v) => $enumDecodeNullable(_$CurrencyEnumMap, v),
        ),
        type: $checkedConvert(
          'type',
          (v) => $enumDecodeNullable(_$LedgerTypeEnumMap, v),
        ),
        account: $checkedConvert(
          'account',
          (v) => $enumDecodeNullable(_$LedgerAccountEnumMap, v),
        ),
        counterpartyAccount: $checkedConvert(
          'counterpartyAccount',
          (v) => $enumDecodeNullable(_$LedgerAccountEnumMap, v),
        ),
        description: $checkedConvert('description', (v) => v as String?),
        correlationId: $checkedConvert('correlationId', (v) => v as String?),
        createdAt: $checkedConvert(
          'createdAt',
          (v) => v == null ? null : DateTime.parse(v as String),
        ),
      );
      return val;
    });

Map<String, dynamic> _$LedgerResponseToJson(
  LedgerResponse instance,
) => <String, dynamic>{
  'id': ?instance.id,
  'walletId': ?instance.walletId,
  'amountMinor': ?instance.amountMinor,
  'currency': ?_$CurrencyEnumMap[instance.currency],
  'type': ?_$LedgerTypeEnumMap[instance.type],
  'account': ?_$LedgerAccountEnumMap[instance.account],
  'counterpartyAccount': ?_$LedgerAccountEnumMap[instance.counterpartyAccount],
  'description': ?instance.description,
  'correlationId': ?instance.correlationId,
  'createdAt': ?instance.createdAt?.toIso8601String(),
};

const _$CurrencyEnumMap = {
  Currency.EUR: 0,
  Currency.USD: 1,
  Currency.PLN: 2,
  Currency.GBP: 3,
};

const _$LedgerTypeEnumMap = {LedgerType.credit: 1, LedgerType.debit: 2};

const _$LedgerAccountEnumMap = {
  LedgerAccount.cash: 1,
  LedgerAccount.clearing: 2,
};
