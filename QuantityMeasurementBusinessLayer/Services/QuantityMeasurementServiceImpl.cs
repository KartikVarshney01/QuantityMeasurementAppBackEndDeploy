using QuantityMeasurementAppBusinessLayer.Engines;
using QuantityMeasurementAppBusinessLayer.Exceptions;
using QuantityMeasurementAppBusinessLayer.Interfaces;
using QuantityMeasurementAppModelLayer.DTOs;
using QuantityMeasurementAppModelLayer.Entities;
using QuantityMeasurementAppModelLayer.Enums;
using QuantityMeasurementAppModelLayer.Models;
using QuantityMeasurementAppRepoLayer.Interfaces;

namespace QuantityMeasurementAppBusinessLayer.Services;

/// <summary>
/// UC18 service implementation.
/// Extends UC17 by accepting a <c>userId</c> parameter on every operation so
/// each persisted record is associated with the authenticated user who made the call.
/// History queries are also scoped to the requesting user.
/// </summary>
public class QuantityMeasurementServiceImpl : IQuantityMeasurementService
{
    private readonly IQuantityMeasurementRepository _repository;

    public QuantityMeasurementServiceImpl(IQuantityMeasurementRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    // ── COMPARE ──────────────────────────────────────────────────────

    public bool Compare(QuantityDTO q1, QuantityDTO q2, long userId)
    {
        ValidationEngine.ValidateSameMeasurement(q1, q2);

        try
        {
            double base1  = ConversionEngine.ConvertToBase(q1);
            double base2  = ConversionEngine.ConvertToBase(q2);
            bool   result = Math.Abs(base1 - base2) < 1e-9;

            _repository.Save(new QuantityMeasurementEntity(
                userId, OperationType.Compare.ToString(),
                ToModel(q1), ToModel(q2), result));

            return result;
        }
        catch (QuantityMeasurementException) { throw; }
        catch (Exception ex)
        {
            SaveError(userId, OperationType.Compare, ex.Message, q1, q2);
            throw new QuantityMeasurementException($"Compare failed: {ex.Message}", ex);
        }
    }

    // ── CONVERT ──────────────────────────────────────────────────────

    public QuantityDTO Convert(QuantityDTO q1, string targetUnit, long userId)
    {
        ArgumentNullException.ThrowIfNull(q1);
        ValidationEngine.ValidateTargetUnit(targetUnit);

        try
        {
            double baseValue = ConversionEngine.ConvertToBase(q1);
            double converted = ConversionEngine.ConvertFromBase(q1.Category, targetUnit, baseValue);
            var    result    = new QuantityDTO(Math.Round(converted, 4), targetUnit, q1.Category);

            _repository.Save(new QuantityMeasurementEntity(
                userId, OperationType.Convert.ToString(),
                ToModel(q1), result.ToString()!));

            return result;
        }
        catch (QuantityMeasurementException) { throw; }
        catch (Exception ex)
        {
            SaveError(userId, OperationType.Convert, ex.Message, q1);
            throw new QuantityMeasurementException($"Convert failed: {ex.Message}", ex);
        }
    }

    // ── ADD ──────────────────────────────────────────────────────────

    public QuantityDTO Add(QuantityDTO q1, QuantityDTO q2, long userId)
    {
        ValidationEngine.ValidateSameMeasurement(q1, q2);

        try
        {
            double base1      = ConversionEngine.ConvertToBase(q1);
            double base2      = ConversionEngine.ConvertToBase(q2);
            double baseResult = ArithmeticEngine.Add(base1, base2, q1.Category);
            double final      = ConversionEngine.ConvertFromBase(q1.Category, q1.UnitName, baseResult);
            var    result     = new QuantityDTO(Math.Round(final, 4), q1.UnitName, q1.Category);

            _repository.Save(new QuantityMeasurementEntity(
                userId, OperationType.Add.ToString(),
                ToModel(q1), ToModel(q2), result.ToString()!));

            return result;
        }
        catch (QuantityMeasurementException) { throw; }
        catch (Exception ex)
        {
            SaveError(userId, OperationType.Add, ex.Message, q1, q2);
            throw new QuantityMeasurementException($"Add failed: {ex.Message}", ex);
        }
    }

    // ── SUBTRACT ─────────────────────────────────────────────────────

    public QuantityDTO Subtract(QuantityDTO q1, QuantityDTO q2, long userId)
    {
        ValidationEngine.ValidateSameMeasurement(q1, q2);

        try
        {
            double base1      = ConversionEngine.ConvertToBase(q1);
            double base2      = ConversionEngine.ConvertToBase(q2);
            double baseResult = ArithmeticEngine.Subtract(base1, base2, q1.Category);
            double final      = ConversionEngine.ConvertFromBase(q1.Category, q1.UnitName, baseResult);
            var    result     = new QuantityDTO(Math.Round(final, 4), q1.UnitName, q1.Category);

            _repository.Save(new QuantityMeasurementEntity(
                userId, OperationType.Subtract.ToString(),
                ToModel(q1), ToModel(q2), result.ToString()!));

            return result;
        }
        catch (QuantityMeasurementException) { throw; }
        catch (Exception ex)
        {
            SaveError(userId, OperationType.Subtract, ex.Message, q1, q2);
            throw new QuantityMeasurementException($"Subtract failed: {ex.Message}", ex);
        }
    }

    // ── DIVIDE ───────────────────────────────────────────────────────

    public double Divide(QuantityDTO q1, QuantityDTO q2, long userId)
    {
        ValidationEngine.ValidateSameMeasurement(q1, q2);

        try
        {
            double base1  = ConversionEngine.ConvertToBase(q1);
            double base2  = ConversionEngine.ConvertToBase(q2);
            double result = ArithmeticEngine.Divide(base1, base2, q1.Category);

            _repository.Save(new QuantityMeasurementEntity(
                userId, OperationType.Divide.ToString(),
                ToModel(q1), ToModel(q2), result));

            return result;
        }
        catch (QuantityMeasurementException) { throw; }
        catch (Exception ex)
        {
            SaveError(userId, OperationType.Divide, ex.Message, q1, q2);
            throw new QuantityMeasurementException($"Divide failed: {ex.Message}", ex);
        }
    }

    // ── HISTORY ──────────────────────────────────────────────────────

    public List<QuantityMeasurementEntity> GetHistory(long userId)
        => _repository.GetAll(userId);

    public List<QuantityMeasurementEntity> GetByOperation(string op, long userId)
        => _repository.GetByOperation(op, userId);

    public List<QuantityMeasurementEntity> GetByMeasurementType(string t, long userId)
        => _repository.GetByMeasurementType(t, userId);

    public int GetCount(long userId)
        => _repository.GetTotalCount(userId);

    // ── Private helpers ───────────────────────────────────────────────

    // QuantityModel now lives in QuantityMeasurementAppModelLayer.Models
    private static QuantityModel<object> ToModel(QuantityDTO dto)
        => new(dto.Value, (object)dto.UnitName);

    private void SaveError(long userId, OperationType op, string message,
                           QuantityDTO? q1 = null, QuantityDTO? q2 = null)
    {
        try
        {
            var entity = new QuantityMeasurementEntity(userId, op.ToString(), message);
            if (q1 != null) entity.Operand1 = ToModel(q1);
            if (q2 != null) entity.Operand2 = ToModel(q2);
            _repository.Save(entity);
        }
        catch
        {
            // Swallow — never hide the original exception with a persistence failure
        }
    }
}
