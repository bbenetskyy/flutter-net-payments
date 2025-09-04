// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'update_card_request.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

UpdateCardRequest _$UpdateCardRequestFromJson(Map<String, dynamic> json) =>
    $checkedCreate('UpdateCardRequest', json, ($checkedConvert) {
      final val = UpdateCardRequest(
        type: $checkedConvert(
          'type',
          (v) => $enumDecodeNullable(_$CardTypeEnumMap, v),
        ),
        name: $checkedConvert('name', (v) => v as String?),
        singleTransactionLimit: $checkedConvert(
          'singleTransactionLimit',
          (v) => (v as num?)?.toDouble(),
        ),
        monthlyLimit: $checkedConvert(
          'monthlyLimit',
          (v) => (v as num?)?.toDouble(),
        ),
        options: $checkedConvert('options', (v) => optionsFromJson(v)),
        printed: $checkedConvert('printed', (v) => v as bool?),
        assignedUserId: $checkedConvert('assignedUserId', (v) => v as String?),
        assignedUserIdSet: $checkedConvert(
          'assignedUserIdSet',
          (v) => v as bool?,
        ),
      );
      return val;
    });

Map<String, dynamic> _$UpdateCardRequestToJson(UpdateCardRequest instance) =>
    <String, dynamic>{
      'type': ?_$CardTypeEnumMap[instance.type],
      'name': ?instance.name,
      'singleTransactionLimit': ?instance.singleTransactionLimit,
      'monthlyLimit': ?instance.monthlyLimit,
      'options': ?optionsToJson(instance.options),
      'printed': ?instance.printed,
      'assignedUserId': ?instance.assignedUserId,
      'assignedUserIdSet': ?instance.assignedUserIdSet,
    };

const _$CardTypeEnumMap = {CardType.Personal: 0, CardType.Shared: 1};
