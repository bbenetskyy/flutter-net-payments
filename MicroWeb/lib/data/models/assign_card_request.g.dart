// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'assign_card_request.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

AssignCardRequest _$AssignCardRequestFromJson(Map<String, dynamic> json) =>
    $checkedCreate('AssignCardRequest', json, ($checkedConvert) {
      final val = AssignCardRequest(
        userId: $checkedConvert('userId', (v) => v as String?),
      );
      return val;
    });

Map<String, dynamic> _$AssignCardRequestToJson(AssignCardRequest instance) =>
    <String, dynamic>{'userId': ?instance.userId};
